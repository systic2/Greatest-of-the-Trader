using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GreatestOfTheTrader.Safety
{
    [CreateAssetMenu(fileName = "HttpSafetyTelemetrySink", menuName = "GreatestOfTheTrader/Safety/HTTP Sink")]
    public class SafetyHttpTelemetrySink : SafetyTelemetrySink
    {
        [Header("Endpoints")]
        [SerializeField] string transitionUrl = "https://api.example.com/safety/events";
        [SerializeField] string metricsUrl = "https://api.example.com/safety/metrics";

        [Header("Headers")]
        [SerializeField] string apiKeyHeader = "X-API-Key";
        [SerializeField] string apiKeyValue;

        [Header("Options")]
        [SerializeField] bool logRequests;

        public override void SendTransition(SafetyStateController.SafetyState fromState,
            SafetyStateController.SafetyState toState,
            SafetyTelemetryReporter.StateTransitionPayload payload)
        {
            if (string.IsNullOrEmpty(transitionUrl))
            {
                return;
            }

            var body = JsonUtility.ToJson(new TransitionDto
            {
                fromState = fromState.ToString(),
                toState = toState.ToString(),
                timestamp = payload.timestamp.ToString("o"),
                metrics = MetricsDto.FromSnapshot(payload.metrics)
            });

            DispatchRequest(transitionUrl, body);
        }

        public override void SendMetrics(SafetyTelemetryReporter.MetricsSnapshot payload)
        {
            if (string.IsNullOrEmpty(metricsUrl))
            {
                return;
            }

            var body = JsonUtility.ToJson(MetricsDto.FromSnapshot(payload));
            DispatchRequest(metricsUrl, body);
        }

        void DispatchRequest(string url, string body)
        {
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(apiKeyValue) && !string.IsNullOrEmpty(apiKeyHeader))
            {
                request.SetRequestHeader(apiKeyHeader, apiKeyValue);
            }

            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (logRequests)
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"[SafetyTelemetry] HTTP success {url} ({request.responseCode})");
                    }
                    else
                    {
                        Debug.LogWarning($"[SafetyTelemetry] HTTP failed {url} ({request.result}) code={request.responseCode} error={request.error}");
                    }
                }

                request.Dispose();
            };
        }

        [System.Serializable]
        class MetricsDto
        {
            public string windowStart;
            public string windowEnd;
            public int sampleCount;
            public float avgSpeed;
            public float avgTilt;
            public float minProximity;
            public string dominantState;

            public static MetricsDto FromSnapshot(SafetyTelemetryReporter.MetricsSnapshot snapshot)
            {
                return new MetricsDto
                {
                    windowStart = snapshot.windowStart.ToString("o"),
                    windowEnd = snapshot.windowEnd.ToString("o"),
                    sampleCount = snapshot.sampleCount,
                    avgSpeed = snapshot.avgSpeed,
                    avgTilt = snapshot.avgTilt,
                    minProximity = snapshot.minProximity,
                    dominantState = snapshot.dominantState.ToString()
                };
            }
        }

        [System.Serializable]
        class TransitionDto
        {
            public string fromState;
            public string toState;
            public string timestamp;
            public MetricsDto metrics;
        }
    }
}
