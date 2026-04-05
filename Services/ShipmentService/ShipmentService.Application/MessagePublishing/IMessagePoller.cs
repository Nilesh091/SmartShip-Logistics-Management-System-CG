using ShipmentService.Domain.Events;

namespace ShipmentService.Application.MessagePublishing;

/// <summary>
/// Interface for polling pending messages from message queue for a specific shipment
/// </summary>
public interface IMessagePoller
{
  /// <summary>
  /// Check and consume pending messages for a specific shipment from the queue
  /// </summary>
  /// <param name="shipmentId">The shipment ID to poll for updates</param>
  /// <returns>List of pending shipment status change events for the shipment</returns>
  Task<List<ShipmentStatusChangedEvent>> PollPendingMessagesAsync(Guid shipmentId);

  /// <summary>
  /// Get the count of pending messages for a specific shipment
  /// </summary>
  /// <param name="shipmentId">The shipment ID to check</param>
  /// <returns>Number of pending messages for the shipment</returns>
  Task<int> GetPendingMessageCountAsync(Guid shipmentId);
}
