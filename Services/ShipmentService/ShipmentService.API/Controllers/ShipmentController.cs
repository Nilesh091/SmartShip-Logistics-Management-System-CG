using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Services;

namespace ShipmentService.API.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentController : ControllerBase
{
  private readonly IShipmentService _service;
  private readonly ILogger<ShipmentController> _logger;

  public ShipmentController(IShipmentService service, ILogger<ShipmentController> logger)
  {
    _service = service;
    _logger = logger;
  }

  /// <summary>
  /// Create a new shipment (CUSTOMER only)
  /// </summary>
  [HttpPost]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentDto dto)
  {
    try
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var userId = GetUserIdFromClaims();
      if (userId == Guid.Empty)
        return Unauthorized("User ID not found in token.");

      _logger.LogInformation($"User {userId} creating new shipment");
      var shipmentId = await _service.CreateShipmentAsync(dto, userId);

      return Ok(new { id = shipmentId, message = "Shipment created successfully." });
    }
    catch (ArgumentException ex)
    {
      _logger.LogWarning($"Validation error: {ex.Message}");
      return BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error creating shipment: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while creating the shipment." });
    }
  }

  /// <summary>
  /// Get all shipments for the current user (CUSTOMER only)
  /// </summary>
  [HttpGet("my")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> GetMyShipments()
  {
    try
    {
      var userId = GetUserIdFromClaims();
      if (userId == Guid.Empty)
        return Unauthorized("User ID not found in token.");

      _logger.LogInformation($"Fetching shipments for user {userId}");
      var shipments = await _service.GetUserShipmentsAsync(userId);

      return Ok(new { data = shipments, count = shipments.Count });
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error fetching user shipments: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while fetching shipments." });
    }
  }

  /// <summary>
  /// Get a specific shipment by ID (CUSTOMER - only their shipments, ADMIN - all)
  /// </summary>
  [HttpGet("{id}")]
  [Authorize]
  public async Task<IActionResult> GetShipmentById(Guid id)
  {
    try
    {
      var shipment = await _service.GetShipmentByIdAsync(id);
      if (shipment == null)
        return NotFound(new { error = "Shipment not found." });

      var userId = GetUserIdFromClaims();
      var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

      // Check authorization: CUSTOMER can only see their own shipments
      if (userRole != "ADMIN" && shipment.UserId != userId)
        return Forbid();

      _logger.LogInformation($"Fetching shipment {id}");
      return Ok(shipment);
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error fetching shipment {id}: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while fetching the shipment." });
    }
  }

  /// <summary>
  /// Book a shipment (transition from Draft to Booked) - CUSTOMER only
  /// </summary>
  [HttpPut("{id}/book")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> BookShipment(Guid id)
  {
    try
    {
      var shipment = await _service.GetShipmentByIdAsync(id);
      if (shipment == null)
        return NotFound(new { error = "Shipment not found." });

      var userId = GetUserIdFromClaims();
      if (shipment.UserId != userId)
        return Forbid("You can only book your own shipments.");

      _logger.LogInformation($"User {userId} booking shipment {id}");
      var success = await _service.BookShipmentAsync(id);

      if (!success)
        return BadRequest(new { error = "Shipment cannot be booked. It may not be in Draft status." });

      return Ok(new { message = "Shipment booked successfully." });
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error booking shipment {id}: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while booking the shipment." });
    }
  }

  /// <summary>
  /// Cancel a shipment (only Draft or Booked) - CUSTOMER only
  /// </summary>
  [HttpDelete("{id}")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> CancelShipment(Guid id)
  {
    try
    {
      var shipment = await _service.GetShipmentByIdAsync(id);
      if (shipment == null)
        return NotFound(new { error = "Shipment not found." });

      var userId = GetUserIdFromClaims();
      if (shipment.UserId != userId)
        return Forbid("You can only cancel your own shipments.");

      _logger.LogInformation($"User {userId} cancelling shipment {id}");
      var success = await _service.CancelShipmentAsync(id);

      if (!success)
        return BadRequest(new { error = "Shipment cannot be cancelled. It may have already been shipped." });

      return Ok(new { message = "Shipment cancelled successfully." });
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error cancelling shipment {id}: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while cancelling the shipment." });
    }
  }

  /// <summary>
  /// Get all shipments (ADMIN only) - for monitoring
  /// </summary>
  [HttpGet]
  [Route("admin/all")]
  [Authorize(Roles = "ADMIN")]
  public async Task<IActionResult> GetAllShipments()
  {
    try
    {
      _logger.LogInformation("Admin fetching all shipments");
      var shipments = await _service.GetAllShipmentsAsync();

      return Ok(new { data = shipments, count = shipments.Count });
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error fetching all shipments: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while fetching shipments." });
    }
  }

  /// <summary>
  /// Update shipment status (ADMIN only) - for lifecycle management
  /// </summary>
  [HttpPut("{id}/status")]
  [Authorize(Roles = "ADMIN")]
  public async Task<IActionResult> UpdateShipmentStatus(Guid id, [FromBody] UpdateShipmentStatusDto dto)
  {
    try
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var shipment = await _service.GetShipmentByIdAsync(id);
      if (shipment == null)
        return NotFound(new { error = "Shipment not found." });

      _logger.LogInformation($"Admin updating shipment {id} status to {dto.Status}");
      var success = await _service.UpdateShipmentStatusAsync(id, dto.Status);

      if (!success)
        return BadRequest(new { error = "Invalid status transition." });

      return Ok(new { message = "Shipment status updated successfully." });
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error updating shipment status for {id}: {ex.Message}");
      return StatusCode(500, new { error = "An error occurred while updating the shipment status." });
    }
  }

  /// <summary>
  /// Helper method to extract UserId from JWT claims
  /// </summary>
  private Guid GetUserIdFromClaims()
  {
    var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
      return userId;

    return Guid.Empty;
  }
}
