using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Domain.Entities;

namespace TrackingService.Infrastructure.Messaging
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("shipment-created", false, false, false);
            channel.QueueDeclare("shipment-status-updated", false, false, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var scope = _scopeFactory.CreateScope();
                var trackingService = scope.ServiceProvider
                    .GetRequiredService<ITrackingService>();

                try
                {
                    if (ea.RoutingKey == "shipment-created")
                    {
                        var evt = JsonSerializer.Deserialize<ShipmentCreatedEvent>(json);
                        if (evt != null)
                            await HandleShipmentCreated(evt, trackingService);
                    }
                    else if (ea.RoutingKey == "shipment-status-updated")
                    {
                        var evt = JsonSerializer.Deserialize<ShipmentStatusUpdatedEvent>(json);
                        if (evt != null)
                            await HandleStatusUpdated(evt, trackingService);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing message: {ex.Message}");
                }
            };

            channel.BasicConsume("shipment-created", true, consumer);
            channel.BasicConsume("shipment-status-updated", true, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleShipmentCreated(ShipmentCreatedEvent evt, ITrackingService trackingService)
        {
            var trackingEvent = new TrackingEvent
            {
                Id = Guid.NewGuid(),
                ShipmentId = evt.ShipmentId,
                Status = evt.Status,
                Location = null,
                Timestamp = evt.CreatedAt
            };

            await trackingService.AddEventAsync(trackingEvent);

            Console.WriteLine($"📦 Tracking created for shipment {evt.ShipmentId}");
        }

        private async Task HandleStatusUpdated(ShipmentStatusUpdatedEvent evt, ITrackingService trackingService)
        {
            var trackingEvent = new TrackingEvent
            {
                Id = Guid.NewGuid(),
                ShipmentId = evt.ShipmentId,
                Status = evt.Status,
                Location = null,
                Timestamp = evt.UpdatedAt
            };

            await trackingService.AddEventAsync(trackingEvent);

            Console.WriteLine($"🚚 Tracking updated for shipment {evt.ShipmentId}: {evt.Status}");
        }
    }
}