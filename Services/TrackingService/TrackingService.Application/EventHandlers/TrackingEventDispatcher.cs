using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Messaging;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Domain.Entities;
namespace TrackingService.Application.EventHandlers
{
    public class TrackingEventDispatcher : IEventDispatcher
    {
        private readonly ITrackingService _trackingService;
        private readonly ILogger<TrackingEventDispatcher> _logger;
        public TrackingEventDispatcher(ITrackingService trackingService, ILogger<TrackingEventDispatcher> logger)
        {
            _trackingService = trackingService;
            _logger = logger;
        }

        public async Task Dispatch(string eventName, string message)
        {
            try
            {
                if (eventName == "shipment-created")
                {
                    var evt = JsonSerializer.Deserialize<ShipmentCreatedEvent>(message);

                    await _trackingService.AddEventAsync(new TrackingEvent
                    {
                        Id = Guid.NewGuid(),
                        ShipmentId = evt.ShipmentId,
                        Status = evt.Status,
                        Timestamp = evt.CreatedAt
                    });
                }
                else if (eventName == "shipment-status-updated")
                {
                    var evt = JsonSerializer.Deserialize<ShipmentStatusUpdatedEvent>(message);

                    await _trackingService.AddEventAsync(new TrackingEvent
                    {
                        Id = Guid.NewGuid(),
                        ShipmentId = evt.ShipmentId,
                        Status = evt.Status,
                        Location = evt.Location,
                        DelayReason = evt.DelayReason,
                        Timestamp = evt.UpdatedAt
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing event: {eventName}");
            }
        }
    }
}
