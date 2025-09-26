using System;
using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    public abstract class SafetyTelemetrySink : ScriptableObject
    {
        public abstract void SendTransition(SafetyStateController.SafetyState fromState,
            SafetyStateController.SafetyState toState,
            SafetyTelemetryReporter.StateTransitionPayload payload);

        public abstract void SendMetrics(SafetyTelemetryReporter.MetricsSnapshot payload);
    }
}
