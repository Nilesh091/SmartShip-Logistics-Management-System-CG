namespace ShipmentService.Domain.Entities;

public class Shipment
{
  public Guid Id { get; set; }

  public Guid UserId { get; set; }

  public string Status { get; set; } = string.Empty;

  public string? CurrentLocation { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime? UpdatedAt { get; set; }

  public Guid SenderAddressId { get; set; }

  public Address? SenderAddress { get; set; }

  public Guid ReceiverAddressId { get; set; }

  public Address? ReceiverAddress { get; set; }

  public Guid PackageId { get; set; }

  public Package? Package { get; set; }

  public decimal Price { get; set; }
}
