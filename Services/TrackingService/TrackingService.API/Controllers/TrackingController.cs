using Microsoft.AspNetCore.Mvc;
using TrackingService.API.DTOs;
using TrackingService.Application.Services.Interfaces;

namespace TrackingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
  private readonly ITrackingService _trackingService;
  private readonly ILogger<TrackingController> _logger;

  public TrackingController(ITrackingService trackingService, ILogger<TrackingController> logger)
  {
    _trackingService = trackingService;
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
      _logger.LogError(ex, "Error retrieving tracking for shipment {ShipmentId}", shipmentId);
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
      var validStatuses = new[] { "DRAFT", "BOOKED", "PICKED_UP", "IN_TRANSIT", "OUT_FOR_DELIVERY", "DELIVERED" };
      if (!validStatuses.Contains(request.Status.ToUpperInvariant()))
      {
        return BadRequest(new { message = $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}" });
      }

      await _trackingService.UpdateShipmentStatusAsync(shipmentId, request.Status, request.Location);

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
}
