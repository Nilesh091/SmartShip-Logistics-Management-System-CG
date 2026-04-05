using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ShipmentService.Domain.Events;

namespace ShipmentService.Infrastructure.MessagePublishing;

public class RabbitMQConsumer : IAsyncDisposable
{
  private readonly string _hostname;
  private readonly string _queueName;
  private IConnection? _connection;
  private IChannel? _channel;
  private readonly ILogger<RabbitMQConsumer>? _logger;

  public RabbitMQConsumer(
    string hostname = "localhost",
    string queueName = "shipment-status-changed-event-events",
    ILogger<RabbitMQConsumer>? logger = null)
  {
    _hostname = hostname;
    _queueName = queueName;
    _logger = logger;
  }

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
            autoDelete: false,
            arguments: null
        );

        // Declare the queue (idempotent - creates if doesn't exist)
        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind queue to exchange with routing key
        var routingKey = "shipment.status.changed";
        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: exchange,
            routingKey: routingKey,
            arguments: null
        );
        _logger?.LogInformation("Queue {Queue} bound to exchange {Exchange} with routing key {RoutingKey}", _queueName, exchange, routingKey);

        // Set QoS to process one message at a time
        await _channel.BasicQosAsync(0, 1, false);
      }
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to connect to RabbitMQ at {Hostname}", _hostname);
      throw new InvalidOperationException($"Failed to connect to RabbitMQ at {_hostname}", ex);
    }
  }

  public async Task StartListeningAsync(
    Func<ShipmentStatusChangedEvent, Task> messageHandler,
    CancellationToken cancellationToken = default)
  {
    try
    {
      await EnsureConnectionAsync();

      _logger?.LogInformation("Starting to listen on queue {QueueName}", _queueName);

      // Create an async consumer
      var consumer = new AsyncShipmentStatusConsumer(_channel!, messageHandler, _logger);

      // Use async consumer to receive messages
      await _channel!.BasicConsumeAsync(
          queue: _queueName,
          autoAck: false, // Manual acknowledgment
          consumerTag: "shipment-status-consumer",
          noLocal: false,
          exclusive: false,
          arguments: null,
          consumer: consumer,
          cancellationToken: cancellationToken
      );

      _logger?.LogInformation("Consumer started successfully for queue {QueueName}", _queueName);

      // Keep listening until cancellation
      await Task.Delay(Timeout.Infinite, cancellationToken);
    }
    catch (OperationCanceledException)
    {
      _logger?.LogInformation("Consumer listening cancelled");
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error in consumer listening");
      throw;
    }
  }

  public async ValueTask DisposeAsync()
  {
    if (_channel != null && _channel.IsOpen)
    {
      try
      {
        await _channel.CloseAsync();
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Error closing channel");
      }
      _channel.Dispose();
    }

    if (_connection != null && _connection.IsOpen)
    {
      try
      {
        await _connection.CloseAsync();
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Error closing connection");
      }
      _connection.Dispose();
    }
  }
}

/// <summary>
/// Async consumer implementation for processing shipment status changed events
/// </summary>
internal class AsyncShipmentStatusConsumer : IAsyncBasicConsumer
{
  private readonly IChannel _channel;
  private readonly Func<ShipmentStatusChangedEvent, Task> _messageHandler;
  private readonly ILogger<RabbitMQConsumer>? _logger;

  public AsyncShipmentStatusConsumer(
    IChannel channel,
    Func<ShipmentStatusChangedEvent, Task> messageHandler,
    ILogger<RabbitMQConsumer>? logger)
  {
    _channel = channel;
    _messageHandler = messageHandler;
    _logger = logger;
  }

  public IChannel Channel => _channel;

  public string? ConsumerTag { get; set; }

  public async Task HandleBasicCancelAsync(string consumerTag, CancellationToken ct)
  {
    _logger?.LogInformation("Consumer {ConsumerTag} was cancelled", consumerTag);
    await Task.CompletedTask;
  }

  public async Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken ct)
  {
    _logger?.LogInformation("Consumer {ConsumerTag} cancel acknowledged", consumerTag);
    await Task.CompletedTask;
  }

  public async Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken ct)
  {
    _logger?.LogInformation("Consumer {ConsumerTag} started consuming", consumerTag);
    await Task.CompletedTask;
  }

  public async Task HandleBasicDeliverAsync(
    string consumerTag,
    ulong deliveryTag,
    bool redelivered,
    string exchange,
    string routingKey,
    IReadOnlyBasicProperties properties,
    ReadOnlyMemory<byte> body,
    CancellationToken ct)
  {
    try
    {
      var json = Encoding.UTF8.GetString(body.Span);
      _logger?.LogInformation("Received message on routing key {RoutingKey}", routingKey);

      var @event = JsonSerializer.Deserialize<ShipmentStatusChangedEvent>(json);

      if (@event != null)
      {
        await _messageHandler(@event);

        // Acknowledge the message after successful processing
        await _channel.BasicAckAsync(deliveryTag, false);
        _logger?.LogInformation("Message acknowledged for shipment {ShipmentId}", @event.ShipmentId);
      }
      else
      {
        _logger?.LogWarning("Failed to deserialize message from routing key {RoutingKey}", routingKey);
        // Negative acknowledge to requeue the message
        await _channel.BasicNackAsync(deliveryTag, false, true);
      }
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Error processing message with delivery tag {DeliveryTag}", deliveryTag);
      // Negative acknowledge to requeue the message
      try
      {
        await _channel.BasicNackAsync(deliveryTag, false, true);
      }
      catch (Exception nackEx)
      {
        _logger?.LogError(nackEx, "Failed to NACK message");
      }
    }
  }

  public async Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason)
  {
    _logger?.LogWarning("Channel shutdown: {Reason}", reason.Cause);
    await Task.CompletedTask;
  }

  public async Task HandleConsumerShutdownAsync(object channel, ShutdownEventArgs reason)
  {
    _logger?.LogWarning("Consumer shutdown: {Reason}", reason.Cause);
    await Task.CompletedTask;
  }

  public async Task HandleModelShutdownAsync(object channel, ShutdownEventArgs reason)
  {
    _logger?.LogWarning("Model shutdown: {Reason}", reason.Cause);
    await Task.CompletedTask;
  }
}