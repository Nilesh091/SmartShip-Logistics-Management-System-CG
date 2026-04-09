namespace TrackingService.Application.Services.Interfaces;

/// <summary>
/// Service interface for managing delivery proofs (POD - Proof of Delivery)
/// </summary>
public interface IDeliveryProofService
{
  /// <summary>
  /// Submit delivery proof for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <param name="recipientName">Name of recipient</param>
  /// <param name="deliveryLocation">Delivery address/location</param>
  /// <param name="signatureFile">Recipient signature image stream (optional)</param>
  /// <param name="proofImage">Proof of delivery image stream (optional)</param>
  /// <param name="signatureFileName">Signature file name (optional)</param>
  /// <param name="proofImageFileName">Proof image file name (optional)</param>
  /// <returns>Delivery proof metadata</returns>
  Task<DeliveryProofMetadata> SubmitDeliveryProofAsync(
      Guid shipmentId,
      string recipientName,
      string? deliveryLocation = null,
      Stream? signatureFile = null,
      Stream? proofImage = null,
      string? signatureFileName = null,
      string? proofImageFileName = null);

  /// <summary>
  /// Get delivery proof for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <returns>Delivery proof metadata if exists</returns>
  Task<DeliveryProofMetadata?> GetDeliveryProofAsync(Guid shipmentId);

  /// <summary>
  /// Delete delivery proof
  /// </summary>
  /// <param name="podId">POD ID</param>
  /// <returns>True if deleted successfully</returns>
  Task<bool> DeleteDeliveryProofAsync(Guid podId);
}

/// <summary>
/// Delivery proof metadata
/// </summary>
public class DeliveryProofMetadata
{
  public Guid PodId { get; set; }
  public Guid ShipmentId { get; set; }
  public string RecipientName { get; set; } = string.Empty;
  public string? DeliveryLocation { get; set; }
  public string? SignatureFilePath { get; set; }
  public string? ProofImageFilePath { get; set; }
  public DateTime DeliveryTimestamp { get; set; }
}
