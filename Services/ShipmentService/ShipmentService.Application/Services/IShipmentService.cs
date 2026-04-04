using ShipmentService.Application.DTOs;

namespace ShipmentService.Application.Services;

public interface IShipmentService
{
  Task<Guid> CreateShipmentAsync(CreateShipmentDto dto, Guid userId);

  Task<ShipmentResponseDto?> GetShipmentByIdAsync(Guid id);

  Task<List<ShipmentResponseDto>> GetUserShipmentsAsync(Guid userId);

  Task<List<ShipmentResponseDto>> GetAllShipmentsAsync();

  Task<bool> BookShipmentAsync(Guid id);

  Task<bool> UpdateShipmentStatusAsync(Guid id, string status);

  Task<bool> CancelShipmentAsync(Guid id);
}
