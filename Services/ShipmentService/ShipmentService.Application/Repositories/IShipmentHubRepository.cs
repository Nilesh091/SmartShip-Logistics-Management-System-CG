using ShipmentService.Domain.Entities;

namespace ShipmentService.Application.Repositories;

public interface IShipmentHubRepository
{
    Task AddRangeAsync(List<ShipmentHub> hubs);
    Task<List<ShipmentHub>> GetByShipmentIdAsync(Guid shipmentId);
    Task<bool> UpdateHubStatusAsync(Guid hubId, string status);
    Task DeleteByShipmentIdAsync(Guid shipmentId);
}
