namespace TrackingService.Application.Services.Interfaces;

/// <summary>
/// Service interface for managing shipment documents
/// Documents include: Invoice, Shipping Labels, etc.
/// </summary>
public interface IDocumentService
{
  /// <summary>
  /// Upload a document for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <param name="documentType">Type of document (Invoice, ShippingLabel)</param>
  /// <param name="file">File stream</param>
  /// <param name="fileName">Original file name</param>
  /// <returns>Document metadata including file path and ID</returns>
  Task<DocumentMetadata> UploadDocumentAsync(Guid shipmentId, string documentType, Stream file, string fileName);

  /// <summary>
  /// Get all documents for a shipment
  /// </summary>
  /// <param name="shipmentId">Shipment ID</param>
  /// <returns>List of documents for the shipment</returns>
  Task<List<DocumentMetadata>> GetDocumentsByShipmentIdAsync(Guid shipmentId);

  /// <summary>
  /// Delete a document
  /// </summary>
  /// <param name="documentId">Document ID</param>
  /// <returns>True if deleted successfully</returns>
  Task<bool> DeleteDocumentAsync(Guid documentId);
}

/// <summary>
/// Document metadata
/// </summary>
public class DocumentMetadata
{
  public Guid DocumentId { get; set; }
  public Guid ShipmentId { get; set; }
  public string DocumentType { get; set; } = string.Empty;
  public string FileName { get; set; } = string.Empty;
  public string FilePath { get; set; } = string.Empty;
  public long FileSize { get; set; }
  public DateTime UploadedAt { get; set; }
}
