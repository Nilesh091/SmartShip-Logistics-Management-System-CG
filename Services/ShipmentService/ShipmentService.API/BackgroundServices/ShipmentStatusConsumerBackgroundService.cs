using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShipmentService.Application.MessagePublishing;

namespace ShipmentService.API.BackgroundServices;

/// <summary>
/// Background service that consumes shipment status change events from RabbitMQ
/// and updates the database.
/// 
/// ⚠️ IMPORTANT: Uses IServiceScopeFactory to resolve scoped IMessageConsumer
/// because BackgroundService is a singleton and cannot directly depend on scoped services.
/// </summary>
public sealed class ShipmentStatusConsumerBackgroundService : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly ILogger<ShipmentStatusConsumerBackgroundService> _logger;

  public ShipmentStatusConsumerBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ShipmentStatusConsumerBackgroundService> logger)
  {
    _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("ShipmentStatusConsumerBackgroundService starting");

    try
    {
      // Create a scope to resolve the scoped IMessageConsumer
      using (var scope = _scopeFactory.CreateScope())
      {
        var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        _logger.LogInformation("Message consumer resolved from scope");

        await messageConsumer.StartConsumingAsync(stoppingToken);
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("ShipmentStatusConsumerBackgroundService stopping");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "ShipmentStatusConsumerBackgroundService encountered an error");
      throw;
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("ShipmentStatusConsumerBackgroundService is stopping");
    await base.StopAsync(cancellationToken);
  }
}
