using System;

namespace Shared.Messaging
{
    public interface IRabbitMQPublisher
    {
        Task PublishAsync<T>(string queue, T message);
        Task InitializeAsync();
    }
}
