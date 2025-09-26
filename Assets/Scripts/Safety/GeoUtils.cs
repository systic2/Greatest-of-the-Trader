using System;
using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    public static class GeoUtils
    {
        const double EarthRadiusMeters = 6371000; // average earth radius

        public static float HaversineDistance(LocationInfo a, LocationInfo b)
        {
            var lat1 = DegreesToRadians(a.latitude);
            var lat2 = DegreesToRadians(b.latitude);
            var deltaLat = DegreesToRadians(b.latitude - a.latitude);
            var deltaLon = DegreesToRadians(b.longitude - a.longitude);

            var sinLat = Math.Sin(deltaLat / 2);
            var sinLon = Math.Sin(deltaLon / 2);

            var h = sinLat * sinLat + Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon;
            var c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));

            return (float)(EarthRadiusMeters * c);
        }

        static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
