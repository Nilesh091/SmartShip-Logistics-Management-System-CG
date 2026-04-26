namespace Shared.Events;

public class ShipmentStatusUpdatedEvent
{
  public Guid ShipmentId { get; set; }
  public string Status { get; set; }
  public string? Location { get; set; }
  public DateTime UpdatedAt { get; set; }
}