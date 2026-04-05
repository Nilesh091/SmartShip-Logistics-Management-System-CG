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

  /// <summary>
  /// Check and consume pending updates from message queue for a specific shipment
  /// </summary>
  /// <param name="shipmentId">The shipment ID to check for pending updates</param>
  /// <returns>The updated shipment response if pending updates were processed</returns>
  Task<ShipmentResponseDto?> ConsumePendingUpdatesAndGetShipmentAsync(Guid shipmentId);

  /// <summary>
  /// Get pending message count for a specific shipment
  /// </summary>
  /// <param name="shipmentId">The shipment ID to check</param>
  /// <returns>Number of pending messages</returns>
  Task<int> GetPendingMessageCountAsync(Guid shipmentId);
}
