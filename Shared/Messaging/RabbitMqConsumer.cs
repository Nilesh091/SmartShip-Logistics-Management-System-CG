using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Messaging
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        private IConnection _connection;
        private IModel _channel;

        public RabbitMqConsumer(
            IConfiguration configuration,
            ILogger<RabbitMqConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq";

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare("shipment-created", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("shipment-status-updated", durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var scope = _serviceProvider.CreateScope();

                var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

                await dispatcher.Dispatch(ea.RoutingKey, json);
            };

            _channel.BasicConsume("shipment-created", true, consumer);
            _channel.BasicConsume("shipment-status-updated", true, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
