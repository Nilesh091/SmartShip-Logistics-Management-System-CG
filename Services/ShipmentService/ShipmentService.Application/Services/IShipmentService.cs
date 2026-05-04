using ShipmentService.Application.DTOs;

namespace ShipmentService.Application.Services;

public interface IShipmentService
{
  Task<Guid> CreateShipmentAsync(CreateShipmentDto dto, Guid userId);

  Task<ShipmentResponseDto?> GetShipmentByIdAsync(Guid id);

  Task<List<ShipmentHubDto>> GetShipmentHubsAsync(Guid shipmentId);

  Task<List<ShipmentHubDto>> GenerateShipmentHubsAsync(Guid shipmentId);

  Task<List<ShipmentResponseDto>> GetUserShipmentsAsync(Guid userId);

  Task<List<ShipmentResponseDto>> GetAllShipmentsAsync();

  Task<bool> BookShipmentAsync(Guid id);

  Task<bool> UpdateShipmentStatusAsync(Guid id, string status, string? location = null, string? delayReason = null);

  Task<bool> UpdateShipmentHubStatusAsync(Guid hubId, string status);

  Task<bool> CancelShipmentAsync(Guid id);
}
