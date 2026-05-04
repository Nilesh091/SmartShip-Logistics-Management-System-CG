using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackingService.API.DTOs;
using TrackingService.Application.Services.Interfaces;

namespace TrackingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrackingController : ControllerBase
{
  private readonly ITrackingService _trackingService;
  private readonly IDocumentService _documentService;
  private readonly IDeliveryProofService _deliveryProofService;
  private readonly ILogger<TrackingController> _logger;

  public TrackingController(
      ITrackingService trackingService,
      IDocumentService documentService,
      IDeliveryProofService deliveryProofService,
      ILogger<TrackingController> logger)
  {
    _trackingService = trackingService;
    _documentService = documentService;
    _deliveryProofService = deliveryProofService;
    _logger = logger;
  }

  /// <summary>
  /// Get tracking timeline for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <returns>List of tracking events ordered by timestamp</returns>
  [HttpGet("{shipmentId}")]
  public async Task<IActionResult> GetTracking(Guid shipmentId)
  {
    try
    {
      var data = await _trackingService.GetTrackingAsync(shipmentId);
      _logger.LogInformation("Retrieved {Count} tracking events for shipment {ShipmentId}", data.Count, shipmentId);
      return Ok(data);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving tracking for shipment {ShipmentId}. Exception: {ExceptionMessage}", shipmentId, ex.Message);

      // Return detailed error in development, generic error in production
      if (System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
      {
        return StatusCode(500, new
        {
          message = "Error retrieving tracking information",
          error = ex.Message,
          innerError = ex.InnerException?.Message
        });
      }

      return StatusCode(500, new { message = "Error retrieving tracking information" });
    }
  }

  /// <summary>
  /// Update shipment status (Admin Only)
  /// Transitions: BOOKED -> PICKED_UP -> IN_TRANSIT -> OUT_FOR_DELIVERY -> DELIVERED
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <param name="request">Status update request with new status and optional location</param>
  /// <returns>Success message with tracking event details</returns>
  [HttpPut("{shipmentId}/status")]
  public async Task<IActionResult> UpdateShipmentStatus(Guid shipmentId, [FromBody] UpdateShipmentStatusRequest request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Status))
      {
        return BadRequest(new { message = "Status is required" });
      }

      // Validate status against allowed values
      var validStatuses = new[] { "DRAFT", "BOOKED", "PICKED_UP", "IN_TRANSIT", "OUT_FOR_DELIVERY", "DELIVERED", "DELAYED" };
      if (!validStatuses.Contains(request.Status.ToUpperInvariant()))
      {
        return BadRequest(new { message = $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}" });
      }

      await _trackingService.UpdateShipmentStatusAsync(shipmentId, request.Status, request.Location, request.DelayReason);

      _logger.LogInformation(
          "Shipment {ShipmentId} status updated to {Status} at location {Location}",
          shipmentId, request.Status, request.Location ?? "N/A");

      return Ok(new
      {
        message = "Shipment status updated successfully",
        shipmentId = shipmentId,
        status = request.Status,
        location = request.Location,
        timestamp = DateTime.UtcNow
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating shipment {ShipmentId} status", shipmentId);
      return StatusCode(500, new { message = "Error updating shipment status" });
    }
  }

  /// <summary>
  /// Upload document for a shipment (Invoice, Shipping Label, etc.)
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <param name="request">Document upload request with type and file</param>
  /// <returns>Document metadata with file path</returns>
  [HttpPost("{shipmentId}/documents/upload")]
  [Consumes("multipart/form-data")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> UploadDocument(
      Guid shipmentId,
      [FromForm] UploadDocumentRequest request)
  {
    try
    {
      // Validate inputs
      if (request.File == null || request.File.Length == 0)
      {
        return BadRequest(new { message = "File is required" });
      }

      if (string.IsNullOrWhiteSpace(request.DocumentType))
      {
        return BadRequest(new { message = "Document type is required" });
      }

      // Validate file size (max 10MB)
      const long maxFileSize = 10 * 1024 * 1024;
      if (request.File.Length > maxFileSize)
      {
        return BadRequest(new { message = "File size exceeds 10MB limit" });
      }

      // Upload document
      using (var stream = request.File.OpenReadStream())
      {
        var metadata = await _documentService.UploadDocumentAsync(
            shipmentId,
            request.DocumentType,
            stream,
            request.File.FileName);

        var response = new DocumentUploadResponse
        {
          DocumentId = metadata.DocumentId,
          ShipmentId = metadata.ShipmentId,
          DocumentType = metadata.DocumentType,
          FileName = metadata.FileName,
          FilePath = metadata.FilePath,
          FileSize = metadata.FileSize,
          UploadedAt = metadata.UploadedAt
        };

        _logger.LogInformation(
            "Document uploaded for shipment {ShipmentId}: {DocumentType}",
            shipmentId, request.DocumentType);

        return Ok(response);
      }
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning(ex, "Invalid document upload request for shipment {ShipmentId}", shipmentId);
      return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error uploading document for shipment {ShipmentId}", shipmentId);
      return StatusCode(500, new { message = "Error uploading document" });
    }
  }

  /// <summary>
  /// Get all documents for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <returns>List of documents</returns>
  [HttpGet("{shipmentId}/documents")]
  public async Task<IActionResult> GetDocuments(Guid shipmentId)
  {
    try
    {
      var documents = await _documentService.GetDocumentsByShipmentIdAsync(shipmentId);

      var response = documents.Select(d => new DocumentUploadResponse
      {
        DocumentId = d.DocumentId,
        ShipmentId = d.ShipmentId,
        DocumentType = d.DocumentType,
        FileName = d.FileName,
        FilePath = d.FilePath,
        FileSize = d.FileSize,
        UploadedAt = d.UploadedAt
      }).ToList();

      _logger.LogInformation("Retrieved {Count} documents for shipment {ShipmentId}", response.Count, shipmentId);
      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving documents for shipment {ShipmentId}", shipmentId);
      return StatusCode(500, new { message = "Error retrieving documents" });
    }
  }

  /// <summary>
  /// Submit delivery proof (POD) for a shipment
  /// Includes recipient signature and/or proof image
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <param name="request">Delivery proof form data</param>
  /// <returns>Delivery proof metadata</returns>
  [HttpPost("{shipmentId}/delivery-proof")]
  [Consumes("multipart/form-data")]
  public async Task<IActionResult> SubmitDeliveryProof(
      Guid shipmentId,
      [FromForm] SubmitDeliveryProofRequest request)
  {
    try
    {
      // Validate inputs
      if (string.IsNullOrWhiteSpace(request.RecipientName))
      {
        return BadRequest(new { message = "Recipient name is required" });
      }

      if (request.Signature == null && request.ProofImage == null)
      {
        return BadRequest(new { message = "At least one of signature or proof image must be provided" });
      }

      // Validate file sizes
      const long maxFileSize = 10 * 1024 * 1024;
      if ((request.Signature?.Length ?? 0) > maxFileSize || (request.ProofImage?.Length ?? 0) > maxFileSize)
      {
        return BadRequest(new { message = "File size exceeds 10MB limit" });
      }

      // Submit delivery proof
      Stream? signatureStream = null;
      Stream? proofImageStream = null;

      try
      {
        if (request.Signature != null)
        {
          signatureStream = request.Signature.OpenReadStream();
        }

        if (request.ProofImage != null)
        {
          proofImageStream = request.ProofImage.OpenReadStream();
        }

        var metadata = await _deliveryProofService.SubmitDeliveryProofAsync(
            shipmentId,
            request.RecipientName,
            request.DeliveryLocation,
            signatureStream,
            proofImageStream,
            request.Signature?.FileName,
            request.ProofImage?.FileName);

        var response = new DeliveryProofResponse
        {
          PodId = metadata.PodId,
          ShipmentId = metadata.ShipmentId,
          RecipientName = metadata.RecipientName,
          DeliveryLocation = metadata.DeliveryLocation,
          SignatureFilePath = metadata.SignatureFilePath,
          ProofImageFilePath = metadata.ProofImageFilePath,
          DeliveryTimestamp = metadata.DeliveryTimestamp
        };

        _logger.LogInformation(
            "Delivery proof submitted for shipment {ShipmentId} by {RecipientName}",
            shipmentId, request.RecipientName);

        return Ok(response);
      }
      finally
      {
        signatureStream?.Dispose();
        proofImageStream?.Dispose();
      }
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning(ex, "Invalid delivery proof request for shipment {ShipmentId}", shipmentId);
      return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error submitting delivery proof for shipment {ShipmentId}", shipmentId);
      return StatusCode(500, new { message = "Error submitting delivery proof" });
    }
  }

  /// <summary>
  /// Get delivery proof for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <returns>Delivery proof metadata if exists</returns>
  [HttpGet("{shipmentId}/delivery-proof")]
  public async Task<IActionResult> GetDeliveryProof(Guid shipmentId)
  {
    try
    {
      var metadata = await _deliveryProofService.GetDeliveryProofAsync(shipmentId);

      if (metadata == null)
      {
        return NotFound(new { message = "No delivery proof found for this shipment" });
      }

      var response = new DeliveryProofResponse
      {
        PodId = metadata.PodId,
        ShipmentId = metadata.ShipmentId,
        RecipientName = metadata.RecipientName,
        DeliveryLocation = metadata.DeliveryLocation,
        SignatureFilePath = metadata.SignatureFilePath,
        ProofImageFilePath = metadata.ProofImageFilePath,
        DeliveryTimestamp = metadata.DeliveryTimestamp
      };

      _logger.LogInformation("Retrieved delivery proof for shipment {ShipmentId}", shipmentId);
      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving delivery proof for shipment {ShipmentId}", shipmentId);
      return StatusCode(500, new { message = "Error retrieving delivery proof" });
    }
  }
}
