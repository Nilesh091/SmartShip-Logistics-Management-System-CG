// using System.Text;
// using System.Text.Json;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Configuration;
// using Shared.Events;
// using TrackingService.Application.Services.Interfaces;
// using TrackingService.Domain.Entities;

// namespace TrackingService.Infrastructure.Messaging
// {
//     public class RabbitMqConsumer : BackgroundService
//     {
//         private readonly IServiceScopeFactory _scopeFactory;
//         private readonly IConfiguration _configuration;
//         private readonly ILogger<RabbitMqConsumer> _logger;
//         private IConnection _connection;
//         private IModel _channel;

//         public RabbitMqConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<RabbitMqConsumer> logger)
//         {
//             _scopeFactory = scopeFactory;
//             _configuration = configuration;
//             _logger = logger;
//         }

//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             var hostName = _configuration["RabbitMQ:HostName"] ?? Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "rabbitmq";
//             var maxRetries = 5;
//             var retryDelay = TimeSpan.FromSeconds(5);

//             for (int attempt = 0; attempt < maxRetries; attempt++)
//             {
//                 try
//                 {
//                     _logger.LogInformation($"[RabbitMQ] Attempting to connect to {hostName} (attempt {attempt + 1}/{maxRetries})...");

//                     var factory = new ConnectionFactory()
//                     {
//                         HostName = hostName,
//                         AutomaticRecoveryEnabled = true,
//                         NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
//                         RequestedHeartbeat = TimeSpan.FromSeconds(60),
//                         ContinuationTimeout = TimeSpan.FromSeconds(10)
//                     };

//                     _connection = factory.CreateConnection();
//                     _channel = _connection.CreateModel();

//                     _logger.LogInformation("✓ [RabbitMQ] Connected successfully");

//                     _channel.QueueDeclare(queue: "shipment-created", durable: false, exclusive: false, autoDelete: false);
//                     _channel.QueueDeclare(queue: "shipment-status-updated", durable: false, exclusive: false, autoDelete: false);

//                     _logger.LogInformation("[RabbitMQ] Queues declared. Listening for messages...");

//                     var consumer = new EventingBasicConsumer(_channel);

//                     consumer.Received += async (model, ea) =>
//                     {
//                         try
//                         {
//                             var json = Encoding.UTF8.GetString(ea.Body.ToArray());
//                             using var scope = _scopeFactory.CreateScope();
//                             var trackingService = scope.ServiceProvider.GetRequiredService<ITrackingService>();

//                             if (ea.RoutingKey == "shipment-created")
//                             {
//                                 var evt = JsonSerializer.Deserialize<ShipmentCreatedEvent>(json);
//                                 if (evt != null)
//                                     await HandleShipmentCreated(evt, trackingService);
//                             }
//                             else if (ea.RoutingKey == "shipment-status-updated")
//                             {
//                                 var evt = JsonSerializer.Deserialize<ShipmentStatusUpdatedEvent>(json);
//                                 if (evt != null)
//                                     await HandleStatusUpdated(evt, trackingService);
//                             }
//                         }
//                         catch (Exception ex)
//                         {
//                             _logger.LogError($"❌ [RabbitMQ] Error processing message: {ex.Message}");
//                         }
//                     };

//                     _channel.BasicConsume(queue: "shipment-created", autoAck: true, consumer: consumer);
//                     _channel.BasicConsume(queue: "shipment-status-updated", autoAck: true, consumer: consumer);

//                     await Task.Delay(Timeout.Infinite, stoppingToken);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError($"⚠️ [RabbitMQ] Connection failed (attempt {attempt + 1}/{maxRetries}): {ex.Message}");

//                     if (attempt < maxRetries - 1)
//                     {
//                         _logger.LogInformation($"[RabbitMQ] Retrying in {retryDelay.TotalSeconds} seconds...");
//                         await Task.Delay(retryDelay, stoppingToken);
//                     }
//                     else
//                     {
//                         _logger.LogError($"❌ [RabbitMQ] Failed after {maxRetries} attempts. Service continuing without message processing.");
//                         return;
//                     }
//                 }
//             }
//         }

//         private async Task HandleShipmentCreated(ShipmentCreatedEvent evt, ITrackingService trackingService)
//         {
//             try
//             {
//                 var trackingEvent = new TrackingEvent
//                 {
//                     Id = Guid.NewGuid(),
//                     ShipmentId = evt.ShipmentId,
//                     Status = evt.Status,
//                     Location = null,
//                     Timestamp = evt.CreatedAt
//                 };

//                 await trackingService.AddEventAsync(trackingEvent);
//                 _logger.LogInformation($"📦 [RabbitMQ] Tracking created for shipment {evt.ShipmentId}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"❌ Error handling shipment created: {ex.Message}");
//             }
//         }

//         private async Task HandleStatusUpdated(ShipmentStatusUpdatedEvent evt, ITrackingService trackingService)
//         {
//             try
//             {
//                 var trackingEvent = new TrackingEvent
//                 {
//                     Id = Guid.NewGuid(),
//                     ShipmentId = evt.ShipmentId,
//                     Status = evt.Status,
//                     Location = null,
//                     Timestamp = evt.UpdatedAt
//                 };

//                 await trackingService.AddEventAsync(trackingEvent);
//                 _logger.LogInformation($"🚚 [RabbitMQ] Tracking updated for shipment {evt.ShipmentId}: {evt.Status}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"❌ Error handling status updated: {ex.Message}");
//             }
//         }

//         public override async Task StopAsync(CancellationToken cancellationToken)
//         {
//             _logger?.LogInformation("[RabbitMQ] Stopping consumer...");

//             try
//             {
//                 _channel?.Close();
//                 _connection?.Close();
//             }
//             catch (Exception ex)
//             {
//                 _logger?.LogError($"Error closing RabbitMQ: {ex.Message}");
//             }

//             await base.StopAsync(cancellationToken);
//         }
//     }
// }