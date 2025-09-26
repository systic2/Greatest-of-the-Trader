using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    /// <summary>
    /// Placeholder interface between map/POI systems and the safety controller.
    /// Update <see cref="DistanceToNearestHazard"/> each frame based on map data.
    /// </summary>
    public class SafetyZoneSensor : MonoBehaviour
    {
        [Tooltip("Meters to the nearest flagged danger zone; set to PositiveInfinity if none.")]
        public float DistanceToNearestHazard { get; private set; } = float.PositiveInfinity;

        [Tooltip("Meters to the nearest safe zone. Used by map logic, not directly by the controller yet.")]
        public float DistanceToNearestSafeZone { get; private set; } = float.PositiveInfinity;

        public void UpdateProximity(float hazardDistance, float safeZoneDistance)
        {
            DistanceToNearestHazard = Mathf.Max(0f, hazardDistance);
            DistanceToNearestSafeZone = Mathf.Max(0f, safeZoneDistance);
        }

        public void Clear()
        {
            DistanceToNearestHazard = float.PositiveInfinity;
            DistanceToNearestSafeZone = float.PositiveInfinity;
        }
    }
}
