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
            var serviceId = _configuration["RabbitMQ:ServiceId"] ?? "service";

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Events this consumer subscribes to
            var exchanges = new[] { "shipment-created", "shipment-status-updated", "otp-generated" };

            var consumer = new EventingBasicConsumer(_channel);

            foreach (var exchange in exchanges)
            {
                // Declare fanout exchange
                _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: true);

                // Each service gets its own queue per exchange
                var queueName = $"{exchange}.{serviceId}";
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(queue: queueName, exchange: exchange, routingKey: "");
                _channel.BasicConsume(queueName, false, consumer);
            }

            consumer.Received += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var eventName = ea.Exchange; // exchange name = event name

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
                    await dispatcher.Dispatch(eventName, json);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process event {EventName}. Message will be requeued.", eventName);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
