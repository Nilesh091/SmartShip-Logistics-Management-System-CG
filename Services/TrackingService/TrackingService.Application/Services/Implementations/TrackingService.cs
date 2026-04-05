using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Domain.Entities;
using TrackingService.Domain.Events;

namespace TrackingService.Application.Services.Implementations;

public class TrackingService : ITrackingService
{
  private readonly ITrackingRepository _repository;
  private readonly IRabbitMQProducer _producer;

  public TrackingService(ITrackingRepository repository, IRabbitMQProducer producer)
  {
    _repository = repository;
    _producer = producer;
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

    // Publish event to RabbitMQ so ShipmentService and other services can consume
    var @event = new ShipmentStatusChangedEvent
    {
      ShipmentId = shipmentId,
      Status = status,
      Location = location,
      Timestamp = trackingEvent.Timestamp
    };

    await _producer.PublishShipmentStatusChangedAsync(@event);
  }
}
