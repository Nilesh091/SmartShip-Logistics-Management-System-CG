using Microsoft.EntityFrameworkCore;
using ShipmentService.Application.Repositories;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Persistence;

namespace ShipmentService.Infrastructure.Repositories;

public class ShipmentHubRepository : IShipmentHubRepository
{
    private readonly ShipmentDbContext _context;

    public ShipmentHubRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(List<ShipmentHub> hubs)
    {
        _context.ShipmentHubs.AddRange(hubs);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ShipmentHub>> GetByShipmentIdAsync(Guid shipmentId)
    {
        return await _context.ShipmentHubs
            .Where(h => h.ShipmentId == shipmentId)
            .OrderBy(h => h.SequenceNumber)
            .ToListAsync();
    }

    public async Task<bool> UpdateHubStatusAsync(Guid hubId, string status)
    {
        var hub = await _context.ShipmentHubs.FindAsync(hubId);
        if (hub == null) return false;
        hub.Status = status;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task DeleteByShipmentIdAsync(Guid shipmentId)
    {
        var hubs = await _context.ShipmentHubs
            .Where(h => h.ShipmentId == shipmentId)
            .ToListAsync();
        if (hubs.Count > 0)
        {
            _context.ShipmentHubs.RemoveRange(hubs);
            await _context.SaveChangesAsync();
        }
    }
}
