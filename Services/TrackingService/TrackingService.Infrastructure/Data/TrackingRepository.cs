using Microsoft.EntityFrameworkCore;
using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Domain.Entities;

namespace TrackingService.Infrastructure.Data;

public class TrackingRepository : ITrackingRepository
{
  private readonly TrackingDbContext _context;

  public TrackingRepository(TrackingDbContext context)
  {
    _context = context;
  }

  public async Task AddEventAsync(TrackingEvent trackingEvent)
  {
    _context.TrackingEvents.Add(trackingEvent);
    await _context.SaveChangesAsync();
  }

  public async Task<List<TrackingEvent>> GetTrackingByShipmentIdAsync(Guid shipmentId)
  {
    return await _context.TrackingEvents
        .Where(x => x.ShipmentId == shipmentId)
        .OrderBy(x => x.Timestamp)
        .ToListAsync();
  }

  public async Task SaveChangesAsync()
  {
    await _context.SaveChangesAsync();
  }
}
