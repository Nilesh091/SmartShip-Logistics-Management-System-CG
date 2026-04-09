using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Domain.Entities;

namespace TrackingService.Application.Services.Implementations;

public class TrackingService : ITrackingService
{
  private readonly ITrackingRepository _repository;

  public TrackingService(ITrackingRepository repository)
  {
    _repository = repository;
  }

  public async Task AddEventAsync(TrackingEvent trackingEvent)
  {
    trackingEvent.Id = Guid.NewGuid();
    trackingEvent.Timestamp = DateTime.UtcNow;
    await _repository.AddEventAsync(trackingEvent);
  }

  public async Task<List<TrackingEvent>> GetTrackingAsync(Guid shipmentId)
  {
    return await _repository.GetTrackingByShipmentIdAsync(shipmentId);
  }

  public async Task UpdateShipmentStatusAsync(Guid shipmentId, string status, string? location = null)
  {
    // Create tracking event
    var trackingEvent = new TrackingEvent
    {
      Id = Guid.NewGuid(),
      ShipmentId = shipmentId,
      Status = status,
      Location = location,
      Timestamp = DateTime.UtcNow
    };

    // Save to database
    await _repository.AddEventAsync(trackingEvent);
    await _repository.SaveChangesAsync();
  }
}
