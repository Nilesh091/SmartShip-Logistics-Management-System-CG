using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Messaging
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;
            _configuration = configuration;

            Initialize(); // sync init
        }

        private void Initialize()
        {
            try
            {
                var hostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq";
                var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
                var password = _configuration["RabbitMQ:Password"] ?? "guest";

                var factory = new ConnectionFactory()
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ unavailable — events will be skipped until connection is restored");
                // Do NOT throw — allow the service to start without RabbitMQ
            }
        }

        public void Publish<T>(string eventName, T message)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                if (_channel == null || _channel.IsClosed)
                    Initialize();

                // Declare fanout exchange
                _channel.ExchangeDeclare(exchange: eventName, type: ExchangeType.Fanout, durable: true);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                // Publish to exchange, not directly to queue
                _channel.BasicPublish(
                    exchange: eventName,
                    routingKey: "",
                    basicProperties: properties,
                    body: body
                );

                _logger.LogDebug($"Message published to exchange '{eventName}'");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to publish message to exchange '{eventName}' — RabbitMQ may be unavailable");
                // Do NOT throw — publishing is best-effort
            }
        }

        public void Dispose()
        {
            try
            {
                if (_channel != null && _channel.IsOpen)
                    _channel.Close();

                if (_connection != null && _connection.IsOpen)
                    _connection.Close();

                _channel?.Dispose();
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ connection closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while disposing RabbitMQ resources");
            }
        }
    }
}