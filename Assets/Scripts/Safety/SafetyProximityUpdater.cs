using System;
using System.Collections.Generic;
using UnityEngine;

namespace GreatestOfTheTrader.Safety
{
    /// <summary>
    /// Computes the player's proximity to pre-defined hazard/safe zones and forwards the distances
    /// to <see cref="SafetyZoneSensor"/>.
    /// </summary>
    [RequireComponent(typeof(SafetyZoneSensor))]
    public class SafetyProximityUpdater : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] SafetyMapData mapData;

        [Header("Sampling")]
        [SerializeField] float sampleIntervalSeconds = 1f;

        [Header("Location Source")]
        [SerializeField] bool useLocationService = true;
        [SerializeField] Vector2 manualCoordinate = new Vector2(37.5665f, 126.9780f); // Seoul default

        SafetyZoneSensor _sensor;
        float _timer;
        bool _locationStarted;

        void Awake()
        {
            _sensor = GetComponent<SafetyZoneSensor>();
        }

        void OnEnable()
        {
            if (useLocationService && !_locationStarted && Input.location.isEnabledByUser)
            {
                Input.location.Start();
                _locationStarted = true;
            }
        }

        void OnDisable()
        {
            if (_locationStarted && Input.location.status == LocationServiceStatus.Running)
            {
                Input.location.Stop();
            }
            _locationStarted = false;
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < sampleIntervalSeconds)
            {
                return;
            }
            _timer = 0f;

            if (mapData == null || mapData.hazardZones.Count == 0)
            {
                _sensor.Clear();
                return;
            }

            if (!TryGetPlayerCoordinate(out var latitude, out var longitude))
            {
                _sensor.Clear();
                return;
            }

            var hazardDistance = FindNearestDistance(mapData.hazardZones, latitude, longitude);
            var safeDistance = mapData.safeZones.Count > 0
                ? FindNearestDistance(mapData.safeZones, latitude, longitude)
                : float.PositiveInfinity;

            _sensor.UpdateProximity(hazardDistance, safeDistance);
        }

        bool TryGetPlayerCoordinate(out double latitude, out double longitude)
        {
            if (useLocationService && Input.location.status == LocationServiceStatus.Running)
            {
                var data = Input.location.lastData;
                if (Math.Abs(data.latitude) > float.Epsilon || Math.Abs(data.longitude) > float.Epsilon)
                {
                    latitude = data.latitude;
                    longitude = data.longitude;
                    return true;
                }
            }

            latitude = manualCoordinate.x;
            longitude = manualCoordinate.y;
            return true;
        }

        float FindNearestDistance(List<GeoZone> zones, double lat, double lon)
        {
            var min = float.PositiveInfinity;
            foreach (var zone in zones)
            {
                var distance = GeoUtils.HaversineDistance(lat, lon, zone.latitude, zone.longitude) - zone.radius;
                min = Mathf.Min(min, Mathf.Max(0f, distance));
            }
            return min;
        }
    }
}
