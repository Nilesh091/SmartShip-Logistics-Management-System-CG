using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ShipmentService.Domain.Events;
using ShipmentService.Application.MessagePublishing;

namespace ShipmentService.Infrastructure.MessagePublishing;

/// <summary>
/// Polls and consumes pending messages from RabbitMQ for specific shipments
/// </summary>
public class RabbitMQMessagePoller : IMessagePoller, IAsyncDisposable
{
  private readonly string _hostname;
  private readonly string _queueName;
  private IConnection? _connection;
  private IChannel? _channel;
  private readonly ILogger<RabbitMQMessagePoller>? _logger;

  public RabbitMQMessagePoller(
      string hostname = "localhost",
      string queueName = "shipment-status-changed-event-events",
      ILogger<RabbitMQMessagePoller>? logger = null)
  {
    _hostname = hostname;
    _queueName = queueName;
    _logger = logger;
  }

  /// <summary>
  /// Ensure RabbitMQ connection and channel are established
  /// </summary>
  private async Task EnsureConnectionAsync()
  {
    try
    {
      if (_connection == null || !_connection.IsOpen)
      {
        var factory = new ConnectionFactory
        {
          HostName = _hostname,
          AutomaticRecoveryEnabled = true,
          NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = await factory.CreateConnectionAsync();
      }

      if (_channel == null || _channel.IsClosed)
      {
        _channel = await _connection!.CreateChannelAsync();

        // Declare the exchange
        var exchange = "shipment-events";
        await _channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: "topic",
            durable: true,
            autoDelete: false
        );

        // Declare the queue
        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Bind queue to exchange with routing key
        var routingKey = "shipment.status.changed";
        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: exchange,
            routingKey: routingKey
        );

        _logger?.LogInformation("Message poller connected to queue {Queue}", _queueName);
      }
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error ensuring RabbitMQ connection in message poller");
      throw;
    }
  }

  /// <summary>
  /// Poll pending messages for a specific shipment and consume them
  /// </summary>
  public async Task<List<ShipmentStatusChangedEvent>> PollPendingMessagesAsync(Guid shipmentId)
  {
    var pendingEvents = new List<ShipmentStatusChangedEvent>();

    try
    {
      await EnsureConnectionAsync();

      // Consume messages from the queue without auto-ack
      while (true)
      {
        try
        {
          // Try to get one message with timeout of 100ms
          var result = await _channel!.BasicGetAsync(_queueName, false);

          if (result == null)
          {
            // No more messages in queue
            break;
          }

          // Deserialize the message
          var body = result.Body.ToArray();
          var messageBody = Encoding.UTF8.GetString(body);

          var @event = JsonSerializer.Deserialize<ShipmentStatusChangedEvent>(messageBody);

          if (@event != null && @event.ShipmentId == shipmentId)
          {
            // This message is for the requested shipment, add to pending
            pendingEvents.Add(@event);
            _logger?.LogInformation("Polled pending message for shipment {ShipmentId}: status {Status}", shipmentId, @event.Status);

            // Acknowledge the message (remove from queue)
            await _channel!.BasicAckAsync(result.DeliveryTag, false);
          }
          else
          {
            // Message is not for this shipment, requeue it (return to queue)
            await _channel!.BasicNackAsync(result.DeliveryTag, false, true);
            _logger?.LogDebug("Requeued message not matching shipment {ShipmentId}", shipmentId);
          }
        }
        catch (OperationCanceledException)
        {
          // Timeout reached, no more messages
          break;
        }
      }

      if (pendingEvents.Any())
      {
        _logger?.LogInformation("Found {Count} pending message(s) for shipment {ShipmentId}", pendingEvents.Count, shipmentId);
      }
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error polling pending messages for shipment {ShipmentId}", shipmentId);
      throw;
    }

    return pendingEvents;
  }

  /// <summary>
  /// Get count of pending messages for a shipment without consuming them
  /// </summary>
  public async Task<int> GetPendingMessageCountAsync(Guid shipmentId)
  {
    try
    {
      await EnsureConnectionAsync();

      int count = 0;
      var messages = new List<BasicGetResult>();

      // Peek at messages without auto-ack
      while (true)
      {
        try
        {
          var result = await _channel!.BasicGetAsync(_queueName, false);

          if (result == null)
          {
            break;
          }

          messages.Add(result);

          // Deserialize to check if it matches
          var body = result.Body.ToArray();
          var messageBody = Encoding.UTF8.GetString(body);
          var @event = JsonSerializer.Deserialize<ShipmentStatusChangedEvent>(messageBody);

          if (@event != null && @event.ShipmentId == shipmentId)
          {
            count++;
          }
        }
        catch (OperationCanceledException)
        {
          break;
        }
      }

      // Requeue all messages we peeked at
      foreach (var msg in messages)
      {
        await _channel!.BasicNackAsync(msg.DeliveryTag, false, true);
      }

      return count;
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error getting pending message count for shipment {ShipmentId}", shipmentId);
      return 0;
    }
  }

  public async ValueTask DisposeAsync()
  {
    if (_channel != null && !_channel.IsClosed)
    {
      await _channel.CloseAsync();
      _channel.Dispose();
    }

    if (_connection != null && _connection.IsOpen)
    {
      await _connection.CloseAsync();
      _connection.Dispose();
    }
  }
}
