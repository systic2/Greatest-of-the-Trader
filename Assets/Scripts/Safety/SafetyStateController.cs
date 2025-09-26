using System;
using UnityEngine;
using UnityEngine.Events;

namespace GreatestOfTheTrader.Safety
{
    /// <summary>
    /// Central state machine that evaluates player context (speed, device tilt, proximity)
    /// and triggers safety warnings aligned with the documented UX spec.
    /// </summary>
    public class SafetyStateController : MonoBehaviour
    {
        public enum SafetyState
        {
            Idle,
            Caution,
            Danger,
            Override
        }

        [Serializable]
        public struct Thresholds
        {
            [Header("Speed (m/s)")]
            public float cautionSpeed;
            public float dangerSpeed;

            [Header("Tilt (degrees)")]
            public float cautionTilt;
            public float dangerTilt;

            [Header("Timing (seconds)")]
            public float dangerHoldDuration;
            public float dangerOverrideDuration;
            public float overrideCooldown;
        }

        [Serializable]
        public struct ProximityConfig
        {
            public float cautionRadius;
            public float dangerRadius;
        }

        [Header("Configuration")]
        public Thresholds thresholds = new Thresholds
        {
            cautionSpeed = 0.5f,
            dangerSpeed = 1.8f,
            cautionTilt = 30f,
            dangerTilt = 60f,
            dangerHoldDuration = 2f,
            dangerOverrideDuration = 10f,
            overrideCooldown = 30f
        };

        public ProximityConfig proximity = new ProximityConfig
        {
            cautionRadius = 15f,
            dangerRadius = 5f
        };

        [Header("Sensor References (optional)")]
        [Tooltip("Assign a component that keeps track of distance to the nearest registered hazard. If not provided, the controller assumes no proximity risk.")]
        public SafetyZoneSensor zoneSensor;

        [Tooltip("Assign to listen to location updates. If null, Unity LocationService is used.")]
        public VelocitySensor velocitySensor;

        [Tooltip("Raised whenever the safety state changes.")]
        public SafetyStateEvent onStateChanged;

        [Tooltip("Raised every frame with the current contextual metrics (speed, tilt, proximity)")]
        public SafetyMetricsEvent onMetricsUpdated;

        [Serializable]
        public class SafetyStateEvent : UnityEvent<SafetyState> { }

        [Serializable]
        public class SafetyMetricsEvent : UnityEvent<SafetyMetrics> { }

        public struct SafetyMetrics
        {
            public float speed;
            public float tilt;
            public float proximity;
            public SafetyState state;
        }

        public SafetyState CurrentState => _currentState;

        float _dangerTimer;
        float _overrideTimer;
        bool _overrideCoolingDown;
        SafetyState _currentState;
        float _currentSpeed;
        float _currentTilt;
        float _currentProximity;

        void Awake()
        {
            if (!SystemInfo.supportsGyroscope)
            {
                Debug.LogWarning("SafetyStateController: Device gyro not available. Tilt-based checks will be skipped.");
            }
            else
            {
                Input.gyro.enabled = true;
            }

            if (velocitySensor == null)
            {
                velocitySensor = gameObject.AddComponent<VelocitySensor>();
            }
        }

        void OnEnable()
        {
            velocitySensor?.BeginTracking();
        }

        void OnDisable()
        {
            velocitySensor?.EndTracking();
        }

        void Update()
        {
            SampleContext(Time.deltaTime);
            var nextState = EvaluateState(Time.deltaTime);

            if (nextState != _currentState)
            {
                TransitionTo(nextState);
            }

            onMetricsUpdated?.Invoke(new SafetyMetrics
            {
                speed = _currentSpeed,
                tilt = _currentTilt,
                proximity = _currentProximity,
                state = _currentState
            });
        }

        void SampleContext(float deltaTime)
        {
            _currentSpeed = velocitySensor != null
                ? velocitySensor.CurrentSpeed
                : 0f;

            if (SystemInfo.supportsGyroscope)
            {
                // angle between gravity vector and device forward (assuming portrait use)
                var gravity = Input.gyro.gravity;
                _currentTilt = Vector3.Angle(gravity, Vector3.down);
            }
            else
            {
                _currentTilt = 0f;
            }

            _currentProximity = zoneSensor != null
                ? zoneSensor.DistanceToNearestHazard
                : float.PositiveInfinity;

            if (_currentState == SafetyState.Override)
            {
                _overrideTimer += deltaTime;
                if (_overrideTimer >= thresholds.overrideCooldown)
                {
                    _overrideCoolingDown = false;
                }
            }
        }

        SafetyState EvaluateState(float deltaTime)
        {
            var proxDanger = _currentProximity <= proximity.dangerRadius;
            var proxCaution = _currentProximity <= proximity.cautionRadius;
            var speedDanger = _currentSpeed >= thresholds.dangerSpeed;
            var speedCaution = _currentSpeed >= thresholds.cautionSpeed;
            var tiltDanger = _currentTilt >= thresholds.dangerTilt;
            var tiltCaution = _currentTilt >= thresholds.cautionTilt;

            if (_overrideCoolingDown)
            {
                return SafetyState.Override;
            }

            if (speedDanger || tiltDanger || proxDanger)
            {
                _dangerTimer += deltaTime;
                if (_dangerTimer >= thresholds.dangerHoldDuration)
                {
                    if (_currentState == SafetyState.Danger)
                    {
                        if (_overrideTimer >= thresholds.dangerOverrideDuration)
                        {
                            _overrideCoolingDown = true;
                            _overrideTimer = 0f;
                            return SafetyState.Override;
                        }
                    }

                    return SafetyState.Danger;
                }
            }
            else
            {
                _dangerTimer = Mathf.Max(0f, _dangerTimer - deltaTime);
            }

            if (speedCaution || tiltCaution || proxCaution)
            {
                ResetOverrideTimerIfNecessary();
                return SafetyState.Caution;
            }

            ResetOverrideTimerIfNecessary();
            return SafetyState.Idle;
        }

        void ResetOverrideTimerIfNecessary()
        {
            if (_currentState != SafetyState.Danger)
            {
                _overrideTimer = 0f;
            }
        }

        void TransitionTo(SafetyState nextState)
        {
            _currentState = nextState;

            if (nextState == SafetyState.Danger)
            {
                _overrideTimer = Mathf.Min(_overrideTimer + _dangerTimer, thresholds.dangerOverrideDuration);
            }
            else if (nextState == SafetyState.Override)
            {
                _overrideCoolingDown = true;
                _overrideTimer = 0f;
            }
            else
            {
                _dangerTimer = 0f;
                _overrideTimer = 0f;
                _overrideCoolingDown = false;
            }

            onStateChanged?.Invoke(_currentState);
        }
    }
}
