using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Domain.Events;

namespace TrackingService.Infrastructure.Messaging;

/// <summary>
/// Infrastructure implementation of RabbitMQ producer
/// Implements the Application layer interface
/// </summary>
public class RabbitMQProducer : IRabbitMQProducer
{
  private readonly IConnectionFactory _connectionFactory;
  private readonly ILogger<RabbitMQProducer> _logger;
  private IConnection? _connection;
  private IChannel? _channel;

  public RabbitMQProducer(IConnectionFactory connectionFactory, ILogger<RabbitMQProducer> logger)
  {
    _connectionFactory = connectionFactory;
    _logger = logger;
  }

  private async Task EnsureConnectionAsync()
  {
    if (_connection == null || !_connection.IsOpen)
    {
      _connection = await _connectionFactory.CreateConnectionAsync();
    }

    if (_channel == null || !_channel.IsOpen)
    {
      _channel = await _connection.CreateChannelAsync();

      // Declare the exchange and queue to ensure they exist
      await _channel.ExchangeDeclareAsync(
          exchange: "shipment-events",
          type: ExchangeType.Topic,
          durable: true,
          autoDelete: false);

      await _channel.QueueDeclareAsync(
          queue: "shipment-status-changed-event-events",
          durable: true,
          exclusive: false,
          autoDelete: false);

      await _channel.QueueBindAsync(
          queue: "shipment-status-changed-event-events",
          exchange: "shipment-events",
          routingKey: "shipment.status.changed");
    }
  }

  public async Task PublishShipmentStatusChangedAsync(ShipmentStatusChangedEvent @event)
  {
    try
    {
      await EnsureConnectionAsync();

      var json = JsonSerializer.Serialize(@event);
      var body = Encoding.UTF8.GetBytes(json);

      var properties = new BasicProperties
      {
        ContentType = "application/json",
        Persistent = true
      };

      await _channel!.BasicPublishAsync(
          exchange: "shipment-events",
          routingKey: "shipment.status.changed",
          mandatory: false,
          basicProperties: properties,
          body: new ReadOnlyMemory<byte>(body));

      _logger.LogInformation(
          "Published shipment status changed event for shipment {ShipmentId} with status {Status}",
          @event.ShipmentId, @event.Status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error publishing shipment status changed event");
      throw;
    }
  }

  public async Task CloseAsync()
  {
    if (_channel != null && _channel.IsOpen)
    {
      await _channel.CloseAsync();
    }

    if (_connection != null && _connection.IsOpen)
    {
      await _connection.CloseAsync();
    }
  }
}
