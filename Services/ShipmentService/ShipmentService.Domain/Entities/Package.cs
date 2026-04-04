namespace ShipmentService.Domain.Entities;

public class Package
{
  public Guid Id { get; set; }

  public double Weight { get; set; }

  public string Description { get; set; } = string.Empty;
}
