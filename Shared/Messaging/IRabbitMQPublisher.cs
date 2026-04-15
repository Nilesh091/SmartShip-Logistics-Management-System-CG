using System;

namespace Shared.Messaging
{
    public interface IRabbitMQPublisher
    {
        void Publish<T>(string queue, T message);
    }
}
