using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;

namespace ShipmentService.Infrastructure.Persistence;

public class ShipmentDbContext : DbContext
{
  public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
      : base(options)
  {
  }

  public DbSet<Shipment> Shipments { get; set; }

  public DbSet<Address> Addresses { get; set; }

  public DbSet<Package> Packages { get; set; }

  public DbSet<ShipmentHub> ShipmentHubs { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configure Shipment entity
    modelBuilder.Entity<Shipment>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.UserId).IsRequired();
      entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
      entity.Property(e => e.CurrentLocation).HasMaxLength(255);
      entity.Property(e => e.DelayReason).HasMaxLength(500);
      entity.Property(e => e.CreatedAt).IsRequired();
      entity.Property(e => e.UpdatedAt);
      entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");

      // Foreign key relationships
      entity.HasOne(e => e.SenderAddress)
              .WithMany()
              .HasForeignKey(e => e.SenderAddressId)
              .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.ReceiverAddress)
              .WithMany()
              .HasForeignKey(e => e.ReceiverAddressId)
              .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(e => e.Package)
              .WithMany()
              .HasForeignKey(e => e.PackageId)
              .OnDelete(DeleteBehavior.Cascade);

      // Indexes for common queries
      entity.HasIndex(e => e.UserId);
      entity.HasIndex(e => e.Status);
    });

    // Configure Address entity
    modelBuilder.Entity<Address>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
      entity.Property(e => e.Street).IsRequired().HasMaxLength(255);
      entity.Property(e => e.City).IsRequired().HasMaxLength(100);
      entity.Property(e => e.State).IsRequired().HasMaxLength(100);
      entity.Property(e => e.ZipCode).IsRequired().HasMaxLength(20);
    });

    // Configure Package entity
    modelBuilder.Entity<Package>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Weight).IsRequired();
      entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
    });

    // Configure ShipmentHub entity
    modelBuilder.Entity<ShipmentHub>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.ShipmentId).IsRequired();
      entity.Property(e => e.Latitude).IsRequired();
      entity.Property(e => e.Longitude).IsRequired();
      entity.Property(e => e.SequenceNumber).IsRequired();
      entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
      entity.Property(e => e.Name).HasMaxLength(200);
      entity.HasIndex(e => e.ShipmentId);
    });
  }
}
