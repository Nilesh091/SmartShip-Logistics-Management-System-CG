using TrackingService.Domain.Entities;

namespace TrackingService.Application.Services.Interfaces;

public interface ITrackingService
{
  Task AddEventAsync(TrackingEvent trackingEvent);
  Task<List<TrackingEvent>> GetTrackingAsync(Guid shipmentId);
  Task UpdateShipmentStatusAsync(Guid shipmentId, string status, string? location = null);
}
