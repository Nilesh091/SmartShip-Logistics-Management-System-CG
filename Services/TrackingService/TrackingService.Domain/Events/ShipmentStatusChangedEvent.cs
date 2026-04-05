namespace TrackingService.Domain.Events;

/// <summary>
/// Domain event for shipment status changes - shared across services
/// </summary>
public class ShipmentStatusChangedEvent
{
  public Guid ShipmentId { get; set; }
  public string Status { get; set; } = string.Empty;
  public string? Location { get; set; }
  public DateTime Timestamp { get; set; }
}
