namespace TrackingService.API.DTOs;

/// <summary>
/// Response DTO for document upload
/// </summary>
public class DocumentUploadResponse
{
  /// <summary>
  /// Document ID
  /// </summary>
  public Guid DocumentId { get; set; }

  /// <summary>
  /// Shipment ID
  /// </summary>
  public Guid ShipmentId { get; set; }

  /// <summary>
  /// Document type (Invoice, ShippingLabel, etc.)
  /// </summary>
  public string DocumentType { get; set; } = string.Empty;

  /// <summary>
  /// Original file name
  /// </summary>
  public string FileName { get; set; } = string.Empty;

  /// <summary>
  /// File path for retrieval
  /// </summary>
  public string FilePath { get; set; } = string.Empty;

  /// <summary>
  /// File size in bytes
  /// </summary>
  public long FileSize { get; set; }

  /// <summary>
  /// Upload timestamp
  /// </summary>
  public DateTime UploadedAt { get; set; }

  /// <summary>
  /// Success message
  /// </summary>
  public string Message { get; set; } = "Document uploaded successfully";
}
