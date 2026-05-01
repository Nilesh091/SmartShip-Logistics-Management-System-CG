using ShipmentService.Domain.ValueObjects;

namespace ShipmentService.Application.Interfaces;

public interface IGeoRoutingService
{
    Task<LatLng?> GetCoordinatesAsync(string place);
    Task<List<LatLng>> GetRoutePointsAsync(LatLng origin, LatLng destination);
    Task<string?> ReverseGeocodeAsync(LatLng point);
    double CalculateDistanceKm(LatLng origin, LatLng destination);
}
