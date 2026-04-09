namespace TrackingService.API.DTOs;

/// <summary>
/// Response DTO for delivery proof upload
/// </summary>
public class DeliveryProofResponse
{
  /// <summary>
  /// POD ID
  /// </summary>
  public Guid PodId { get; set; }

  /// <summary>
  /// Shipment ID
  /// </summary>
  public Guid ShipmentId { get; set; }

  /// <summary>
  /// Recipient signature file path
  /// </summary>
  public string? SignatureFilePath { get; set; }

  /// <summary>
  /// Proof of delivery image file path
  /// </summary>
  public string? ProofImageFilePath { get; set; }

  /// <summary>
  /// Recipient name
  /// </summary>
  public string RecipientName { get; set; } = string.Empty;

  /// <summary>
  /// Delivery timestamp
  /// </summary>
  public DateTime DeliveryTimestamp { get; set; }

  /// <summary>
  /// Delivery location/address
  /// </summary>
  public string? DeliveryLocation { get; set; }

  /// <summary>
  /// Success message
  /// </summary>
  public string Message { get; set; } = "Delivery proof submitted successfully";
}
