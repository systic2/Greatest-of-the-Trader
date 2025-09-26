using System;
using System.Collections.Generic;
using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    /// <summary>
    /// Collects state transitions and periodic metrics snapshots from SafetyStateController
    /// and forwards them to a configured telemetry sink.
    /// </summary>
    [RequireComponent(typeof(SafetyStateController))]
    public class SafetyTelemetryReporter : MonoBehaviour
    {
        [Header("Telemetry")]
        [SerializeField] SafetyTelemetrySink sink;
        [SerializeField] float metricsIntervalSeconds = 30f;
        [SerializeField] bool logWhenNoSink = true;

        SafetyStateController _controller;
        readonly List<SafetyStateController.SafetyState> _transitionHistory = new();
        SafetyStateController.SafetyState _previousState;
        float _metricsTimer;
        MetricsAccumulator _accumulator;

        [Serializable]
        public struct StateTransitionPayload
        {
            public DateTime timestamp;
            public MetricsSnapshot metrics;
        }

        [Serializable]
        public struct MetricsSnapshot
        {
            public DateTime windowStart;
            public DateTime windowEnd;
            public int sampleCount;
            public float avgSpeed;
            public float avgTilt;
            public float minProximity;
            public SafetyStateController.SafetyState dominantState;
        }

        struct MetricsAccumulator
        {
            public DateTime start;
            public float sumSpeed;
            public float sumTilt;
            public float minProximity;
            public int samples;
            public Dictionary<SafetyStateController.SafetyState, int> stateCounts;

            public void Reset(DateTime timestamp)
            {
                start = timestamp;
                sumSpeed = 0f;
                sumTilt = 0f;
                minProximity = float.PositiveInfinity;
                samples = 0;
                stateCounts ??= new Dictionary<SafetyStateController.SafetyState, int>();
                stateCounts.Clear();
            }

            public void AddSample(SafetyStateController.SafetyState state, float speed, float tilt, float proximity)
            {
                if (samples == 0)
                {
                    start = DateTime.UtcNow;
                    minProximity = proximity;
                }

                samples++;
                sumSpeed += speed;
                sumTilt += tilt;
                minProximity = Mathf.Min(minProximity, proximity);

                if (stateCounts.TryGetValue(state, out var count))
                {
                    stateCounts[state] = count + 1;
                }
                else
                {
                    stateCounts[state] = 1;
                }
            }

            public MetricsSnapshot ToSnapshot(DateTime now)
            {
                var dominant = SafetyStateController.SafetyState.Idle;
                var dominantCount = -1;
                if (stateCounts != null)
                {
                    foreach (var kv in stateCounts)
                    {
                        if (kv.Value > dominantCount)
                        {
                            dominantCount = kv.Value;
                            dominant = kv.Key;
                        }
                    }
                }

                return new MetricsSnapshot
                {
                    windowStart = start,
                    windowEnd = now,
                    sampleCount = samples,
                    avgSpeed = samples > 0 ? sumSpeed / samples : 0f,
                    avgTilt = samples > 0 ? sumTilt / samples : 0f,
                    minProximity = samples > 0 ? minProximity : float.PositiveInfinity,
                    dominantState = dominant
                };
            }
        }

        void Awake()
        {
            _controller = GetComponent<SafetyStateController>();
            _previousState = _controller.CurrentState;
            _accumulator.Reset(DateTime.UtcNow);
        }

        void OnEnable()
        {
            _controller.onStateChanged.AddListener(OnStateChanged);
            _controller.onMetricsUpdated.AddListener(OnMetrics);
        }

        void OnDisable()
        {
            _controller.onStateChanged.RemoveListener(OnStateChanged);
            _controller.onMetricsUpdated.RemoveListener(OnMetrics);
        }

        void Update()
        {
            _metricsTimer += Time.deltaTime;
            if (_metricsTimer >= metricsIntervalSeconds && _accumulator.samples > 0)
            {
                FlushMetrics();
            }
        }

        void OnStateChanged(SafetyStateController.SafetyState newState)
        {
            var payload = new StateTransitionPayload
            {
                timestamp = DateTime.UtcNow,
                metrics = _accumulator.ToSnapshot(DateTime.UtcNow)
            };

            DispatchTransition(_previousState, newState, payload);
            _previousState = newState;
            _metricsTimer = 0f;
            _accumulator.Reset(DateTime.UtcNow);
        }

        void OnMetrics(SafetyStateController.SafetyMetrics metrics)
        {
            _accumulator.AddSample(metrics.state, metrics.speed, metrics.tilt, metrics.proximity);
        }

        void FlushMetrics()
        {
            var snapshot = _accumulator.ToSnapshot(DateTime.UtcNow);
            DispatchMetrics(snapshot);
            _metricsTimer = 0f;
            _accumulator.Reset(DateTime.UtcNow);
        }

        void DispatchTransition(SafetyStateController.SafetyState fromState,
            SafetyStateController.SafetyState toState,
            StateTransitionPayload payload)
        {
            if (sink != null)
            {
                sink.SendTransition(fromState, toState, payload);
            }
            else if (logWhenNoSink)
            {
                Debug.Log($"[SafetyTelemetry] Transition {fromState} -> {toState} speed={payload.metrics.avgSpeed:F2} tilt={payload.metrics.avgTilt:F1}");
            }
        }

        void DispatchMetrics(MetricsSnapshot snapshot)
        {
            if (sink != null)
            {
                sink.SendMetrics(snapshot);
            }
            else if (logWhenNoSink)
            {
                Debug.Log($"[SafetyTelemetry] Metrics window {snapshot.sampleCount} samples avgSpeed={snapshot.avgSpeed:F2} avgTilt={snapshot.avgTilt:F1} minProximity={snapshot.minProximity:F1}");
            }
        }
    }
}
