namespace TrackingService.API.DTOs;

/// <summary>
/// Request DTO for submitting delivery proof (POD)
/// </summary>
public class SubmitDeliveryProofRequest
{
  /// <summary>
  /// Name of the recipient
  /// </summary>
  public string RecipientName { get; set; } = string.Empty;

  /// <summary>
  /// Delivery address/location (optional)
  /// </summary>
  public string? DeliveryLocation { get; set; }

  /// <summary>
  /// Signature image file (optional)
  /// </summary>
  public IFormFile? Signature { get; set; }

  /// <summary>
  /// Proof of delivery image file (optional)
  /// </summary>
  public IFormFile? ProofImage { get; set; }
}
