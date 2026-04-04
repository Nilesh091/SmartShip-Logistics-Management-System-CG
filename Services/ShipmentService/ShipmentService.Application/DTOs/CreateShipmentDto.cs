namespace ShipmentService.Application.DTOs;

public class CreateShipmentDto
{
  public AddressDto Sender { get; set; } = new();

  public AddressDto Receiver { get; set; } = new();

  public PackageDto Package { get; set; } = new();
}
