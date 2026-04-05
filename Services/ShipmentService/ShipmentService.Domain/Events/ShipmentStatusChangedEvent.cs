namespace ShipmentService.Domain.Events;

public class ShipmentStatusChangedEvent
{
  public Guid ShipmentId { get; set; }

  public string Status { get; set; } = string.Empty;

  public DateTime Timestamp { get; set; }

  public Guid UserId { get; set; }
}
