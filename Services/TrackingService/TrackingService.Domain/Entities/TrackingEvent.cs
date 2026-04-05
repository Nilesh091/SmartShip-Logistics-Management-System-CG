namespace TrackingService.Domain.Entities;

public class TrackingEvent
{
  public Guid Id { get; set; }
  public Guid ShipmentId { get; set; }
  public string Status { get; set; } = string.Empty;
  public string? Location { get; set; }
  public DateTime Timestamp { get; set; }
}
