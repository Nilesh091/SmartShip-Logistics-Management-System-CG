namespace ShipmentService.Application.DTOs;

public class ShipmentResponseDto
{
  public Guid Id { get; set; }

  public Guid UserId { get; set; }

  public string Status { get; set; } = string.Empty;

  public string? CurrentLocation { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime? UpdatedAt { get; set; }

  public AddressDto? SenderAddress { get; set; }

  public AddressDto? ReceiverAddress { get; set; }

  public PackageDto? Package { get; set; }
}
