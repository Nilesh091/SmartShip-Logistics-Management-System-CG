using ShipmentService.Domain.Entities;

namespace ShipmentService.Application.Repositories;

public interface IShipmentRepository
{
  Task<Guid> AddAsync(Shipment shipment);

  Task<Shipment?> GetByIdAsync(Guid id);

  Task<List<Shipment>> GetUserShipmentsAsync(Guid userId);

  Task<List<Shipment>> GetAllAsync();

  Task<bool> UpdateAsync(Shipment shipment);

  Task<bool> DeleteAsync(Guid id);

  Task<bool> ExistsAsync(Guid id);
}
