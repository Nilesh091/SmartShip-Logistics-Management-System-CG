namespace TrackingService.API.DTOs;

/// <summary>
/// Request DTO for uploading documents
/// </summary>
public class UploadDocumentRequest
{
  /// <summary>
  /// Type of document (Invoice, ShippingLabel, Other)
  /// </summary>
  public string DocumentType { get; set; } = string.Empty;

  /// <summary>
  /// Document file to upload
  /// </summary>
  public IFormFile File { get; set; } = null!;
}
