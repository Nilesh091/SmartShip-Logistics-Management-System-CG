using System;

namespace ShipmentService.Application.Repositories
{
    public interface IRabbitMQPublisher
    {
        public void Publish<T>(string queue, T message);
    }
}
