using ShipmentService.Domain.Events;

namespace ShipmentService.Application.MessagePublishing;

public interface IMessageConsumer
{
  Task StartConsumingAsync(CancellationToken cancellationToken = default);
}
