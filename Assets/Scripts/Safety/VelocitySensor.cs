using System;
using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    /// <summary>
    /// Estimates the player's ground speed using Unity's LocationService.
    /// Falls back to IMU step counting placeholder if GPS is unavailable.
    /// </summary>
    public class VelocitySensor : MonoBehaviour
    {
        [Tooltip("Minimum delta time (seconds) between GPS samples before we recompute speed")]
        public float sampleInterval = 0.5f;

        [Tooltip("If GPS is unavailable, we decay the cached speed using this coefficient per second")]
        public float fallbackDecay = 2f;

        public float CurrentSpeed { get; private set; }

        LocationInfo _lastLocation;
        bool _lastLocationValid;
        float _lastSampleTime;

        StepEstimator _stepEstimator;

        void Awake()
        {
            _stepEstimator = new StepEstimator();
        }

        public void BeginTracking()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("VelocitySensor: location services disabled; attempting fallback estimation.");
                _stepEstimator.Reset();
                return;
            }

            Input.location.Start();
            _lastSampleTime = Time.time;
        }

        public void EndTracking()
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                Input.location.Stop();
            }
            _stepEstimator.Reset();
            _lastLocationValid = false;
            CurrentSpeed = 0f;
        }

        void Update()
        {
            var now = Time.time;

            if (Input.location.status == LocationServiceStatus.Running)
            {
                var location = Input.location.lastData;
                if (!_lastLocationValid)
                {
                    _lastLocation = location;
                    _lastLocationValid = true;
                    _lastSampleTime = now;
                    return;
                }

                if (now - _lastSampleTime < sampleInterval)
                {
                    return;
                }

                var deltaTime = Mathf.Max(0.01f, (float)(location.timestamp - _lastLocation.timestamp));
                var distance = GeoUtils.HaversineDistance(_lastLocation, location);
                CurrentSpeed = distance / deltaTime;

                _lastLocation = location;
                _lastSampleTime = now;
            }
            else
            {
                // fallback: decay the previous speed and rely on IMU step count if available
                _stepEstimator.Update(now);
                var imuSpeed = _stepEstimator.EstimatedSpeed;
                CurrentSpeed = Mathf.Max(0f, Mathf.Lerp(CurrentSpeed, imuSpeed, Time.deltaTime * 2f));
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, fallbackDecay * Time.deltaTime);
            }
        }

        /// <summary>
        /// Basic IMU-based step estimator placeholder. Replace with platform specific APIs later.
        /// </summary>
        class StepEstimator
        {
            const float StepLengthMeters = 0.75f;
            float _lastUpdateTime;

            public float EstimatedSpeed { get; private set; }

            public void Reset()
            {
                EstimatedSpeed = 0f;
                _lastUpdateTime = Time.time;
            }

            public void Update(float currentTime)
            {
                var deltaTime = Mathf.Max(0.01f, currentTime - _lastUpdateTime);
                _lastUpdateTime = currentTime;

                // Without a real pedometer we cannot produce accurate steps.
                // Keep returning zero for now; future integration point.
                EstimatedSpeed = 0f;
            }
        }
    }
}
