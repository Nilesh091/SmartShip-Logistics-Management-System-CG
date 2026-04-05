using TrackingService.Domain.Entities;

namespace TrackingService.Application.Repositories.Interfaces;

public interface ITrackingRepository
{
  Task AddEventAsync(TrackingEvent trackingEvent);
  Task<List<TrackingEvent>> GetTrackingByShipmentIdAsync(Guid shipmentId);
  Task SaveChangesAsync();
}
