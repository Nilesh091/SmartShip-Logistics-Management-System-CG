using Microsoft.EntityFrameworkCore;
using TrackingService.Domain.Entities;

namespace TrackingService.Infrastructure.Data;

public class TrackingDbContext : DbContext
{
  public DbSet<TrackingEvent> TrackingEvents { get; set; }

  public TrackingDbContext(DbContextOptions<TrackingDbContext> options)
      : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<TrackingEvent>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.ShipmentId).IsRequired();
      entity.Property(e => e.Status).IsRequired();
      entity.Property(e => e.Timestamp).IsRequired();
      entity.Property(e => e.Location).IsRequired(false);
      entity.Property(e => e.DelayReason).IsRequired(false).HasMaxLength(500);
    });
  }
}
