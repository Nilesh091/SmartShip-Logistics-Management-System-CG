using Microsoft.Extensions.Logging;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Infrastructure.Storage;

namespace TrackingService.Infrastructure.Services;

/// <summary>
/// Implementation of document management service using local file storage
/// </summary>
public class DocumentService : IDocumentService
{
  private readonly LocalFileStorageService _fileStorage;
  private readonly ILogger<DocumentService> _logger;

  // In-memory storage for document metadata (in production, use a database)
  private static readonly Dictionary<Guid, DocumentMetadata> DocumentsMetadata = new();

  public DocumentService(LocalFileStorageService fileStorage, ILogger<DocumentService> logger)
  {
    _fileStorage = fileStorage;
    _logger = logger;
  }

  public async Task<DocumentMetadata> UploadDocumentAsync(
      Guid shipmentId,
      string documentType,
      Stream file,
      string fileName)
  {
    try
    {
      // Validate inputs
      if (file == null || file.Length == 0)
      {
        throw new ArgumentException("File cannot be null or empty");
      }

      if (string.IsNullOrWhiteSpace(documentType))
      {
        throw new ArgumentException("Document type is required");
      }

      if (string.IsNullOrWhiteSpace(fileName))
      {
        throw new ArgumentException("File name is required");
      }

      // Validate document type
      var validDocumentTypes = new[] { "Invoice", "ShippingLabel", "Other" };
      if (!validDocumentTypes.Contains(documentType))
      {
        throw new ArgumentException($"Invalid document type. Allowed: {string.Join(", ", validDocumentTypes)}");
      }

      // Save file to local storage
      var (filePath, fileSize) = await _fileStorage.SaveFileAsync(
          "documents",
          shipmentId,
          file,
          fileName);

      // Create metadata
      var documentId = Guid.NewGuid();
      var metadata = new DocumentMetadata
      {
        DocumentId = documentId,
        ShipmentId = shipmentId,
        DocumentType = documentType,
        FileName = fileName,
        FilePath = filePath,
        FileSize = fileSize,
        UploadedAt = DateTime.UtcNow
      };

      // Store metadata
      DocumentsMetadata[documentId] = metadata;

      _logger.LogInformation(
          "Document uploaded for shipment {ShipmentId}: {DocumentType} - {FileName}",
          shipmentId, documentType, fileName);

      return metadata;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error uploading document for shipment {ShipmentId}", shipmentId);
      throw;
    }
  }

  public Task<List<DocumentMetadata>> GetDocumentsByShipmentIdAsync(Guid shipmentId)
  {
    var documents = DocumentsMetadata.Values
        .Where(d => d.ShipmentId == shipmentId)
        .ToList();

    _logger.LogInformation("Retrieved {Count} documents for shipment {ShipmentId}", documents.Count, shipmentId);

    return Task.FromResult(documents);
  }

  public Task<bool> DeleteDocumentAsync(Guid documentId)
  {
    try
    {
      if (DocumentsMetadata.TryGetValue(documentId, out var metadata))
      {
        // Delete file from storage
        var deleted = _fileStorage.DeleteFile(metadata.FilePath);

        // Remove from metadata
        DocumentsMetadata.Remove(documentId);

        _logger.LogInformation("Document {DocumentId} deleted successfully", documentId);
        return Task.FromResult(deleted);
      }

      _logger.LogWarning("Document {DocumentId} not found", documentId);
      return Task.FromResult(false);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
      throw;
    }
  }
}
