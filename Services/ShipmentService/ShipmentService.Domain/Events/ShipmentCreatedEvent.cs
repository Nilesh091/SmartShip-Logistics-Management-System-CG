namespace ShipmentService.Domain.Events;

public class ShipmentCreatedEvent
{
  public Guid ShipmentId { get; set; }

  public Guid UserId { get; set; }

  public string Status { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }
}
