using ShipmentService.Application.Interfaces;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.ValueObjects;

namespace ShipmentService.Infrastructure.Services;

public class HubGeneratorService : IHubGeneratorService
{
    private readonly IGeoRoutingService _geoRouting;

    public HubGeneratorService(IGeoRoutingService geoRouting)
    {
        _geoRouting = geoRouting;
    }

    public async Task<List<ShipmentHub>> GenerateHubsAsync(Guid shipmentId, List<LatLng> routePoints, double thresholdKm = 120)
    {
        if (routePoints.Count < 2)
            return new List<ShipmentHub>();

        // Step 1: collect hub points using Haversine (pure CPU, no I/O)
        var hubPoints = new List<LatLng>();
        double accumulated = 0;

        for (int i = 1; i < routePoints.Count; i++)
        {
            accumulated += Haversine(routePoints[i - 1], routePoints[i]);
            if (accumulated >= thresholdKm)
            {
                hubPoints.Add(routePoints[i]);
                accumulated = 0;
            }
        }

        // Step 2: reverse geocode each hub point with 1.1s delay (Nominatim: 1 req/sec)
        var hubs = new List<ShipmentHub>();
        for (int i = 0; i < hubPoints.Count; i++)
        {
            var point = hubPoints[i];
            var name = await _geoRouting.ReverseGeocodeAsync(point);

            hubs.Add(new ShipmentHub
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipmentId,
                Latitude = point.Lat,
                Longitude = point.Lng,
                Name = name,
                SequenceNumber = i + 1,
                Status = "PENDING"
            });

            // Respect Nominatim rate limit (1 req/sec) — skip delay after last hub
            if (i < hubPoints.Count - 1)
                await Task.Delay(1100);
        }

        return hubs;
    }

    private static double Haversine(LatLng p1, LatLng p2)
    {
        const double R = 6371;
        var dLat = ToRad(p2.Lat - p1.Lat);
        var dLon = ToRad(p2.Lng - p1.Lng);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(p1.Lat)) * Math.Cos(ToRad(p2.Lat))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
