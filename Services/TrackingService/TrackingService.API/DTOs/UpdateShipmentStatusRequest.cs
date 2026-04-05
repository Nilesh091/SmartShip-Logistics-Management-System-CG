namespace TrackingService.API.DTOs;

/// <summary>
/// DTO for updating shipment status
/// </summary>
public class UpdateShipmentStatusRequest
{
  /// <summary>
  /// The new status for the shipment
  /// Valid values: DRAFT, BOOKED, PICKED_UP, IN_TRANSIT, OUT_FOR_DELIVERY, DELIVERED
  /// </summary>
  public string Status { get; set; } = string.Empty;

  /// <summary>
  /// Optional location information (e.g., "Ludhiana" or "Delhi - Hub")
  /// </summary>
  public string? Location { get; set; }
}
