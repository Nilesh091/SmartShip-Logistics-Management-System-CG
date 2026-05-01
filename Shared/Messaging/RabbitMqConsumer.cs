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
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            // Retry until RabbitMQ is available
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _logger.LogInformation("RabbitMQ consumer connected successfully");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ unavailable — retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            if (stoppingToken.IsCancellationRequested) return;

            // Events this consumer subscribes to
            var exchanges = new[] { "shipment-created", "shipment-status-updated", "otp-generated" };

            var consumer = new EventingBasicConsumer(_channel);

            // Wire up handler BEFORE calling BasicConsume
            consumer.Received += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var eventName = ea.Exchange;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
                    await dispatcher.Dispatch(eventName, json);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Processed and acked event {EventName}", eventName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process event {EventName}. Message will be requeued.", eventName);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            foreach (var exchange in exchanges)
            {
                _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: true);

                var staleQueueName = $"{exchange}.service";
                try { _channel.QueueDelete(staleQueueName, ifUnused: false, ifEmpty: false); } catch { }

                var queueName = $"{exchange}.{serviceId}";
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(queue: queueName, exchange: exchange, routingKey: "");
                _channel.BasicConsume(queueName, false, consumer);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
