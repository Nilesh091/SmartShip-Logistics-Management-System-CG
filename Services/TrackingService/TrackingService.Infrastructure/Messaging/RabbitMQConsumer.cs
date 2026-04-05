using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Domain.Entities;
using TrackingService.Domain.Events;

namespace TrackingService.Infrastructure.Messaging;

/// <summary>
/// Infrastructure implementation for consuming RabbitMQ messages
/// </summary>
public class RabbitMQConsumer
{
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly ILogger<RabbitMQConsumer> _logger;
  private IConnection? _connection;
  private IChannel? _channel;

  public RabbitMQConsumer(IServiceScopeFactory scopeFactory, ILogger<RabbitMQConsumer> logger)
  {
    _scopeFactory = scopeFactory;
    _logger = logger;
  }

  public async Task StartAsync(string hostName = "localhost")
  {
    try
    {
      var factory = new ConnectionFactory() { HostName = hostName };
      _connection = await factory.CreateConnectionAsync();
      _channel = await _connection.CreateChannelAsync();

      await _channel.QueueDeclareAsync(queue: "shipment-status-changed-event-events", durable: true, exclusive: false, autoDelete: false);

      var consumer = new AsyncEventingBasicConsumer(_channel);

      consumer.ReceivedAsync += ConsumerOnReceivedAsync;

      await _channel.BasicConsumeAsync(queue: "shipment-status-changed-event-events", autoAck: true, consumerTag: "tracking-service", consumer: consumer);
      _logger.LogInformation("RabbitMQ Consumer started successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to start RabbitMQ Consumer");
      throw;
    }
  }

  private async Task ConsumerOnReceivedAsync(object model, BasicDeliverEventArgs ea)
  {
    try
    {
      var body = ea.Body.ToArray();
      var json = Encoding.UTF8.GetString(body);

      var eventData = JsonSerializer.Deserialize<ShipmentStatusChangedEvent>(json);
      if (eventData == null)
      {
        _logger.LogWarning("Failed to deserialize event message");
        return;
      }

      using var scope = _scopeFactory.CreateScope();
      var service = scope.ServiceProvider.GetRequiredService<ITrackingService>();

      await service.AddEventAsync(new TrackingEvent
      {
        Id = Guid.NewGuid(),
        ShipmentId = eventData.ShipmentId,
        Status = eventData.Status,
        Location = eventData.Location,
        Timestamp = eventData.Timestamp
      });

      _logger.LogInformation("Processed tracking event for shipment {ShipmentId} with status {Status}",
          eventData.ShipmentId, eventData.Status);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing message");
    }
  }

  public async Task StopAsync()
  {
    if (_channel != null)
    {
      await _channel.CloseAsync();
    }
    if (_connection != null)
    {
      await _connection.CloseAsync();
    }
  }
}

