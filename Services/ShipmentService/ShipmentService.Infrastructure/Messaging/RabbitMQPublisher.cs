using System;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ShipmentService.Application.Repositories;
namespace TrackingService.Infrastructure.Messaging
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQPublisher()
        {
            _factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
        }

        public void Publish<T>(string queue, T message)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(exchange: "",
                                 routingKey: queue,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
