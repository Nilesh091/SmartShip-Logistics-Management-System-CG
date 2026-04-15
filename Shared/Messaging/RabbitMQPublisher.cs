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
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        public void Publish<T>(string queue, T message)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                // Ensure connection is alive
                if (_channel == null || _channel.IsClosed)
                {
                    Initialize();
                }

                // Declare queue
                _channel.QueueDeclare(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Serialize message
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                // Properties (persistent message)
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                // Publish
                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queue,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogDebug($"Message published to queue '{queue}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish message to queue '{queue}'");
                throw;
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