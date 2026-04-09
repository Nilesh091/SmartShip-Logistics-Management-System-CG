namespace Shared.Events;

public class ShipmentCreatedEvent
{
  public Guid ShipmentId { get; set; }
  public Guid UserId { get; set; }
  public string Status { get; set; }
  public DateTime CreatedAt { get; set; }
}