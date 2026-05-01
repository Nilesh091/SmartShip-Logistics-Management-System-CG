using ShipmentService.Domain.Entities;
using ShipmentService.Domain.ValueObjects;

namespace ShipmentService.Application.Interfaces;

public interface IHubGeneratorService
{
    Task<List<ShipmentHub>> GenerateHubsAsync(Guid shipmentId, List<LatLng> routePoints, double thresholdKm = 120);
}
