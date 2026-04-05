using Microsoft.Extensions.Logging;
using ShipmentService.Application.MessagePublishing;
using ShipmentService.Application.Repositories;
using ShipmentService.Domain.Events;
using ShipmentService.Infrastructure.MessagePublishing;

namespace ShipmentService.Infrastructure.MessagePublishing;

public class MessageConsumer : IMessageConsumer
{
  private readonly RabbitMQConsumer _rabbitMQConsumer;
  private readonly IShipmentRepository _shipmentRepository;
  private readonly ILogger<MessageConsumer> _logger;

  public MessageConsumer(
    string hostname = "localhost",
    string queueName = "shipment-status-changed-event-events",
    IShipmentRepository? shipmentRepository = null,
    ILogger<MessageConsumer>? logger = null)
  {
    _rabbitMQConsumer = new RabbitMQConsumer(
        hostname,
        queueName,
        logger != null ? new LoggerAdapter(logger) : null
    );
    _shipmentRepository = shipmentRepository ?? throw new ArgumentNullException(nameof(shipmentRepository));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("MessageConsumer starting to consume from RabbitMQ");

      await _rabbitMQConsumer.StartListeningAsync(
        async (shipmentStatusChangedEvent) =>
        {
          await HandleShipmentStatusChangedAsync(shipmentStatusChangedEvent);
        },
        cancellationToken
      );
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("MessageConsumer cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in MessageConsumer");
      throw;
    }
  }

  private async Task HandleShipmentStatusChangedAsync(ShipmentStatusChangedEvent @event)
  {
    try
    {
      _logger.LogInformation(
        "Processing ShipmentStatusChangedEvent for shipment {ShipmentId} with new status {Status}",
        @event.ShipmentId,
        @event.Status
      );

      // Retrieve the shipment from the database
      var shipment = await _shipmentRepository.GetByIdAsync(@event.ShipmentId);

      if (shipment == null)
      {
        _logger.LogWarning("Shipment with ID {ShipmentId} not found", @event.ShipmentId);
        return;
      }

      // Update the shipment status
      shipment.Status = @event.Status;
      var success = await _shipmentRepository.UpdateAsync(shipment);

      if (success)
      {
        _logger.LogInformation(
          "Successfully updated shipment {ShipmentId} status to {Status}",
          @event.ShipmentId,
          @event.Status
        );
      }
      else
      {
        _logger.LogWarning(
          "Failed to update shipment {ShipmentId} status",
          @event.ShipmentId
        );
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Error handling ShipmentStatusChangedEvent for shipment {ShipmentId}",
        @event.ShipmentId
      );
      throw;
    }
  }
}

/// <summary>
/// Adapter to convert ILogger<MessageConsumer> to ILogger<RabbitMQConsumer>
/// </summary>
internal class LoggerAdapter : ILogger<RabbitMQConsumer>
{
  private readonly ILogger<MessageConsumer> _logger;

  public LoggerAdapter(ILogger<MessageConsumer> logger)
  {
    _logger = logger;
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
  public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    _logger.Log(logLevel, eventId, state, exception, formatter);
  }
}

