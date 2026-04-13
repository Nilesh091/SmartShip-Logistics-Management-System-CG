using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Shared.Messaging
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IAsyncDisposable
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
                var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
                var password = _configuration["RabbitMQ:Password"] ?? "guest";

                var factory = new ConnectionFactory()
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        public async Task PublishAsync<T>(string queue, T message)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                // Ensure connection is initialized
                if (_channel == null || !_channel.IsOpen)
                {
                    await InitializeAsync();
                }

                // Declare queue
                await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);

                // Serialize message
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                // Create properties (no separate Properties class needed in v7)
                var properties = new BasicProperties { Persistent = true };

                // Publish message using correct v7 API
                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queue,
                    mandatory: false,
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

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_channel?.IsOpen == true)
                    await _channel.CloseAsync();

                if (_connection?.IsOpen == true)
                    await _connection.CloseAsync();

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
