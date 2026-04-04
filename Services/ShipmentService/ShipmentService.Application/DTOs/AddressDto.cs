namespace ShipmentService.Application.DTOs;

public class AddressDto
{
  public string Name { get; set; } = string.Empty;

  public string Street { get; set; } = string.Empty;

  public string City { get; set; } = string.Empty;

  public string State { get; set; } = string.Empty;

  public string ZipCode { get; set; } = string.Empty;
}
