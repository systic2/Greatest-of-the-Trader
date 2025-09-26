using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    [CreateAssetMenu(fileName = "ConsoleSafetyTelemetrySink", menuName = "GreatestOfTheTrader/Safety/Console Sink")]
    public class ConsoleSafetyTelemetrySink : SafetyTelemetrySink
    {
        public override void SendTransition(SafetyStateController.SafetyState fromState,
            SafetyStateController.SafetyState toState,
            SafetyTelemetryReporter.StateTransitionPayload payload)
        {
            Debug.Log($"[SafetyTelemetry] State {fromState} -> {toState} at {payload.timestamp:O} avgSpeed={payload.metrics.avgSpeed:F2} avgTilt={payload.metrics.avgTilt:F1} minProximity={payload.metrics.minProximity:F1}");
        }

        public override void SendMetrics(SafetyTelemetryReporter.MetricsSnapshot payload)
        {
            Debug.Log($"[SafetyTelemetry] Metrics window {payload.sampleCount} samples avgSpeed={payload.avgSpeed:F2} avgTilt={payload.avgTilt:F1} minProximity={payload.minProximity:F1}");
        }
    }
}
