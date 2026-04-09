using Microsoft.Extensions.Logging;

namespace TrackingService.Infrastructure.Storage;

/// <summary>
/// Local file storage service for managing file operations
/// </summary>
public class LocalFileStorageService
{
  private readonly string _baseStoragePath;
  private readonly ILogger<LocalFileStorageService> _logger;

  public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
  {
    _logger = logger;

    // Set base storage path to "uploads" folder in application directory
    _baseStoragePath = Path.Combine(AppContext.BaseDirectory, "uploads");

    // Create base directory if it doesn't exist
    if (!Directory.Exists(_baseStoragePath))
    {
      Directory.CreateDirectory(_baseStoragePath);
      _logger.LogInformation("Created uploads directory at {Path}", _baseStoragePath);
    }
  }

  /// <summary>
  /// Save a file locally
  /// </summary>
  /// <param name="subDirectory">Subdirectory within uploads (e.g., "documents", "proofs")</param>
  /// <param name="shipmentId">Shipment ID for organizing files</param>
  /// <param name="file">File stream</param>
  /// <param name="fileName">Original file name</param>
  /// <returns>Relative file path</returns>
  public async Task<(string FilePath, long FileSize)> SaveFileAsync(
      string subDirectory,
      Guid shipmentId,
      Stream file,
      string fileName)
  {
    try
    {
      // Create directory structure
      var directoryPath = Path.Combine(_baseStoragePath, subDirectory, shipmentId.ToString());
      if (!Directory.Exists(directoryPath))
      {
        Directory.CreateDirectory(directoryPath);
      }

      // Generate unique file name to avoid conflicts
      var fileExtension = Path.GetExtension(fileName);
      var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
      var fullFilePath = Path.Combine(directoryPath, uniqueFileName);

      // Save file
      using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
      {
        await file.CopyToAsync(fileStream);
      }

      var fileInfo = new FileInfo(fullFilePath);
      var relativePath = Path.Combine(subDirectory, shipmentId.ToString(), uniqueFileName)
          .Replace("\\", "/"); // Normalize path separators

      _logger.LogInformation(
          "File saved successfully: {FileName} ({Size} bytes) at {Path}",
          fileName, fileInfo.Length, relativePath);

      return (relativePath, fileInfo.Length);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error saving file {FileName}", fileName);
      throw;
    }
  }

  /// <summary>
  /// Delete a file
  /// </summary>
  /// <param name="filePath">Relative file path</param>
  /// <returns>True if deleted successfully</returns>
  public bool DeleteFile(string filePath)
  {
    try
    {
      var fullPath = Path.Combine(_baseStoragePath, filePath);

      if (File.Exists(fullPath))
      {
        File.Delete(fullPath);
        _logger.LogInformation("File deleted successfully: {Path}", filePath);
        return true;
      }

      _logger.LogWarning("File not found for deletion: {Path}", filePath);
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting file: {Path}", filePath);
      throw;
    }
  }

  /// <summary>
  /// Check if file exists
  /// </summary>
  public bool FileExists(string filePath)
  {
    var fullPath = Path.Combine(_baseStoragePath, filePath);
    return File.Exists(fullPath);
  }

  /// <summary>
  /// Get full file path
  /// </summary>
  public string GetFullPath(string filePath)
  {
    return Path.Combine(_baseStoragePath, filePath);
  }
}
