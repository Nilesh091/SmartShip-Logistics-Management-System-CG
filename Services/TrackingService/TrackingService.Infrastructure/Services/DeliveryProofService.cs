using Microsoft.Extensions.Logging;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Infrastructure.Storage;

namespace TrackingService.Infrastructure.Services;

/// <summary>
/// Implementation of delivery proof service using local file storage
/// </summary>
public class DeliveryProofService : IDeliveryProofService
{
  private readonly LocalFileStorageService _fileStorage;
  private readonly ILogger<DeliveryProofService> _logger;

  // In-memory storage for POD metadata (in production, use a database)
  private static readonly Dictionary<Guid, DeliveryProofMetadata> DeliveryProofsMetadata = new();
  private static readonly Dictionary<Guid, Guid> ShipmentToPodMap = new(); // Map shipmentId to podId

  public DeliveryProofService(LocalFileStorageService fileStorage, ILogger<DeliveryProofService> logger)
  {
    _fileStorage = fileStorage;
    _logger = logger;
  }

  public async Task<DeliveryProofMetadata> SubmitDeliveryProofAsync(
      Guid shipmentId,
      string recipientName,
      string? deliveryLocation = null,
      Stream? signatureFile = null,
      Stream? proofImage = null,
      string? signatureFileName = null,
      string? proofImageFileName = null)
  {
    try
    {
      // Validate inputs
      if (string.IsNullOrWhiteSpace(recipientName))
      {
        throw new ArgumentException("Recipient name is required");
      }

      if (signatureFile == null && proofImage == null)
      {
        throw new ArgumentException("At least one of signature or proof image must be provided");
      }

      string? signatureFilePath = null;
      string? proofImageFilePath = null;

      // Save signature file if provided
      if (signatureFile != null && !string.IsNullOrWhiteSpace(signatureFileName))
      {
        var (filePath, _) = await _fileStorage.SaveFileAsync(
            "proofs/signatures",
            shipmentId,
            signatureFile,
            signatureFileName);
        signatureFilePath = filePath;
      }

      // Save proof image if provided
      if (proofImage != null && !string.IsNullOrWhiteSpace(proofImageFileName))
      {
        var (filePath, _) = await _fileStorage.SaveFileAsync(
            "proofs/images",
            shipmentId,
            proofImage,
            proofImageFileName);
        proofImageFilePath = filePath;
      }

      // Create metadata
      var podId = Guid.NewGuid();
      var metadata = new DeliveryProofMetadata
      {
        PodId = podId,
        ShipmentId = shipmentId,
        RecipientName = recipientName,
        DeliveryLocation = deliveryLocation,
        SignatureFilePath = signatureFilePath,
        ProofImageFilePath = proofImageFilePath,
        DeliveryTimestamp = DateTime.UtcNow
      };

      // Store metadata
      DeliveryProofsMetadata[podId] = metadata;
      ShipmentToPodMap[shipmentId] = podId; // Allow one POD per shipment

      _logger.LogInformation(
          "Delivery proof submitted for shipment {ShipmentId} by recipient {RecipientName}",
          shipmentId, recipientName);

      return metadata;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error submitting delivery proof for shipment {ShipmentId}", shipmentId);
      throw;
    }
  }

  public Task<DeliveryProofMetadata?> GetDeliveryProofAsync(Guid shipmentId)
  {
    try
    {
      if (ShipmentToPodMap.TryGetValue(shipmentId, out var podId))
      {
        if (DeliveryProofsMetadata.TryGetValue(podId, out var metadata))
        {
          _logger.LogInformation("Retrieved delivery proof for shipment {ShipmentId}", shipmentId);
          return Task.FromResult<DeliveryProofMetadata?>(metadata);
        }
      }

      _logger.LogInformation("No delivery proof found for shipment {ShipmentId}", shipmentId);
      return Task.FromResult<DeliveryProofMetadata?>(null);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving delivery proof for shipment {ShipmentId}", shipmentId);
      throw;
    }
  }

  public Task<bool> DeleteDeliveryProofAsync(Guid podId)
  {
    try
    {
      if (DeliveryProofsMetadata.TryGetValue(podId, out var metadata))
      {
        // Delete files from storage
        if (!string.IsNullOrEmpty(metadata.SignatureFilePath))
        {
          _fileStorage.DeleteFile(metadata.SignatureFilePath);
        }

        if (!string.IsNullOrEmpty(metadata.ProofImageFilePath))
        {
          _fileStorage.DeleteFile(metadata.ProofImageFilePath);
        }

        // Remove from maps
        DeliveryProofsMetadata.Remove(podId);
        ShipmentToPodMap.Remove(metadata.ShipmentId);

        _logger.LogInformation("Delivery proof {PodId} deleted successfully", podId);
        return Task.FromResult(true);
      }

      _logger.LogWarning("Delivery proof {PodId} not found", podId);
      return Task.FromResult(false);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting delivery proof {PodId}", podId);
      throw;
    }
  }
}
