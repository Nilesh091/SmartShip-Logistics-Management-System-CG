using Microsoft.EntityFrameworkCore;
using ShipmentService.Application.Repositories;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Persistence;

namespace ShipmentService.Infrastructure.Repositories;

public class ShipmentRepository : IShipmentRepository
{
  private readonly ShipmentDbContext _context;

  public ShipmentRepository(ShipmentDbContext context)
  {
    _context = context;
  }

  public async Task<Guid> AddAsync(Shipment shipment)
  {
    _context.Shipments.Add(shipment);
    await _context.SaveChangesAsync();
    return shipment.Id;
  }

  public async Task<Shipment?> GetByIdAsync(Guid id)
  {
    return await _context.Shipments
        .Include(s => s.SenderAddress)
        .Include(s => s.ReceiverAddress)
        .Include(s => s.Package)
        .FirstOrDefaultAsync(s => s.Id == id);
  }

  public async Task<List<Shipment>> GetUserShipmentsAsync(Guid userId)
  {
    return await _context.Shipments
        .Where(s => s.UserId == userId)
        .Include(s => s.SenderAddress)
        .Include(s => s.ReceiverAddress)
        .Include(s => s.Package)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync();
  }

  public async Task<List<Shipment>> GetAllAsync()
  {
    return await _context.Shipments
        .Include(s => s.SenderAddress)
        .Include(s => s.ReceiverAddress)
        .Include(s => s.Package)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync();
  }

  public async Task<bool> UpdateAsync(Shipment shipment)
  {
    shipment.UpdatedAt = DateTime.UtcNow;
    _context.Shipments.Update(shipment);
    var result = await _context.SaveChangesAsync();
    return result > 0;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    var shipment = await _context.Shipments.FindAsync(id);
    if (shipment == null)
      return false;

    _context.Shipments.Remove(shipment);
    var result = await _context.SaveChangesAsync();
    return result > 0;
  }

  public async Task<bool> ExistsAsync(Guid id)
  {
    return await _context.Shipments.AnyAsync(s => s.Id == id);
  }
}
