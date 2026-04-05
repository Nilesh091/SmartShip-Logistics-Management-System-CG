using TrackingService.Domain.Events;

namespace TrackingService.Application.Services.Interfaces;

/// <summary>
/// Application layer interface for publishing domain events via RabbitMQ
/// Implementation should be provided by Infrastructure layer
/// </summary>
public interface IRabbitMQProducer
{
  Task PublishShipmentStatusChangedAsync(ShipmentStatusChangedEvent @event);
}
