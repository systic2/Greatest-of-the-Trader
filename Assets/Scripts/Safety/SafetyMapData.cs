using System;
using System.Collections.Generic;
using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    [CreateAssetMenu(fileName = "SafetyMapData", menuName = "GreatestOfTheTrader/Safety/Map Data")]
    public class SafetyMapData : ScriptableObject
    {
        public List<GeoZone> hazardZones = new List<GeoZone>();
        public List<GeoZone> safeZones = new List<GeoZone>();
    }

    [Serializable]
    public struct GeoZone
    {
        public string id;
        [Tooltip("Latitude in decimal degrees")]
        public double latitude;
        [Tooltip("Longitude in decimal degrees")]
        public double longitude;
        [Tooltip("Effective radius in meters")]
        public float radius;
        [Tooltip("Optional display label")]
        public string label;
    }
}
