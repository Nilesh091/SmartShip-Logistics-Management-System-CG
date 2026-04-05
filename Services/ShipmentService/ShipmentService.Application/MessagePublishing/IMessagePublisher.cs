namespace ShipmentService.Application.MessagePublishing;

public interface IMessagePublisher
{
  Task PublishAsync<T>(T message) where T : class;
}
