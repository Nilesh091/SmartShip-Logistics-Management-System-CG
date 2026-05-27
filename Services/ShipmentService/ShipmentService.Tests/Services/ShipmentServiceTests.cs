using Microsoft.Extensions.Logging;
using Moq;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Repositories;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Enums;
using ShipmentService.Domain.ValueObjects;
using Shared.Messaging;

namespace ShipmentService.Tests.Services;

[TestFixture]
public class ShipmentServiceTests
{
    private Mock<IShipmentRepository> _shipmentRepo;
    private Mock<IShipmentHubRepository> _hubRepo;
    private Mock<IGeoRoutingService> _geoRouting;
    private Mock<IHubGeneratorService> _hubGenerator;
    private Mock<IPricingService> _pricing;
    private Mock<IRabbitMQPublisher> _publisher;
    private Mock<ILogger<ShipmentService.Application.Services.ShipmentService>> _logger;
    private ShipmentService.Application.Services.ShipmentService _service;

    [SetUp]
    public void Setup()
    {
        _shipmentRepo = new Mock<IShipmentRepository>();
        _hubRepo = new Mock<IShipmentHubRepository>();
        _geoRouting = new Mock<IGeoRoutingService>();
        _hubGenerator = new Mock<IHubGeneratorService>();
        _pricing = new Mock<IPricingService>();
        _publisher = new Mock<IRabbitMQPublisher>();
        _logger = new Mock<ILogger<ShipmentService.Application.Services.ShipmentService>>();

        _service = new ShipmentService.Application.Services.ShipmentService(
            _shipmentRepo.Object,
            _hubRepo.Object,
            _geoRouting.Object,
            _hubGenerator.Object,
            _pricing.Object,
            _publisher.Object,
            _logger.Object
        );
    }

    // ── CreateShipment ────────────────────────────────────────────────────────

    [Test]
    public async Task CreateShipment_ValidDto_ShouldSaveShipmentAndPublishEvent()
    {
        var shipmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = BuildValidCreateDto();

        _shipmentRepo.Setup(r => r.AddAsync(It.IsAny<Shipment>())).ReturnsAsync(shipmentId);
        _shipmentRepo.Setup(r => r.GetByIdAsync(shipmentId)).ReturnsAsync(BuildShipment(shipmentId));
        _shipmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Shipment>())).ReturnsAsync(true);
        _geoRouting.Setup(g => g.GetCoordinatesAsync(It.IsAny<string>())).ReturnsAsync(new LatLng(28.6, 77.2));
        _geoRouting.Setup(g => g.CalculateDistanceKm(It.IsAny<LatLng>(), It.IsAny<LatLng>())).Returns(500);
        _geoRouting.Setup(g => g.GetRoutePointsAsync(It.IsAny<LatLng>(), It.IsAny<LatLng>())).ReturnsAsync(new List<LatLng>());
        _pricing.Setup(p => p.CalculateFare(500, It.IsAny<double>())).Returns(1500m);
        _hubGenerator.Setup(h => h.GenerateHubsAsync(shipmentId, It.IsAny<List<LatLng>>(), It.IsAny<double>())).ReturnsAsync(new List<ShipmentHub>());

        var result = await _service.CreateShipmentAsync(dto, userId);

        Assert.That(result, Is.EqualTo(shipmentId));
        _shipmentRepo.Verify(r => r.AddAsync(It.IsAny<Shipment>()), Times.Once);
        _publisher.Verify(p => p.Publish("shipment-created", It.IsAny<object>()), Times.Once);
    }

    [Test]
    public void CreateShipment_MissingSenderName_ShouldThrowArgumentException()
    {
        var dto = BuildValidCreateDto();
        dto.Sender.Name = "";

        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateShipmentAsync(dto, Guid.NewGuid()));
    }

    [Test]
    public void CreateShipment_MissingReceiverName_ShouldThrowArgumentException()
    {
        var dto = BuildValidCreateDto();
        dto.Receiver.Name = "";

        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateShipmentAsync(dto, Guid.NewGuid()));
    }

    [Test]
    public void CreateShipment_ZeroPackageWeight_ShouldThrowArgumentException()
    {
        var dto = BuildValidCreateDto();
        dto.Package.Weight = 0;

        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateShipmentAsync(dto, Guid.NewGuid()));
    }

    // ── GenerateShipmentHubs ──────────────────────────────────────────────────

    [Test]
    public async Task GenerateHubs_ValidShipment_ShouldReturnGeneratedHubs()
    {
        var shipmentId = Guid.NewGuid();
        var hubs = new List<ShipmentHub>
        {
            new() { Id = Guid.NewGuid(), ShipmentId = shipmentId, Name = "Hub A", SequenceNumber = 1 },
            new() { Id = Guid.NewGuid(), ShipmentId = shipmentId, Name = "Hub B", SequenceNumber = 2 }
        };

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipmentId)).ReturnsAsync(BuildShipment(shipmentId));
        _shipmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Shipment>())).ReturnsAsync(true);
        _geoRouting.Setup(g => g.GetCoordinatesAsync(It.IsAny<string>())).ReturnsAsync(new LatLng(28.6, 77.2));
        _geoRouting.Setup(g => g.CalculateDistanceKm(It.IsAny<LatLng>(), It.IsAny<LatLng>())).Returns(300);
        _geoRouting.Setup(g => g.GetRoutePointsAsync(It.IsAny<LatLng>(), It.IsAny<LatLng>())).ReturnsAsync(new List<LatLng>());
        _pricing.Setup(p => p.CalculateFare(It.IsAny<double>(), It.IsAny<double>())).Returns(900m);
        _hubGenerator.Setup(h => h.GenerateHubsAsync(shipmentId, It.IsAny<List<LatLng>>(), It.IsAny<double>())).ReturnsAsync(hubs);

        var result = await _service.GenerateShipmentHubsAsync(shipmentId);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Hub A"));
        _hubRepo.Verify(r => r.DeleteByShipmentIdAsync(shipmentId), Times.Once);
        _hubRepo.Verify(r => r.AddRangeAsync(hubs), Times.Once);
    }

    [Test]
    public async Task GenerateHubs_ShipmentNotFound_ShouldReturnEmptyList()
    {
        _shipmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Shipment?)null);

        var result = await _service.GenerateShipmentHubsAsync(Guid.NewGuid());

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GenerateHubs_CoordinatesUnresolvable_ShouldReturnEmptyList()
    {
        var shipmentId = Guid.NewGuid();
        _shipmentRepo.Setup(r => r.GetByIdAsync(shipmentId)).ReturnsAsync(BuildShipment(shipmentId));
        _geoRouting.Setup(g => g.GetCoordinatesAsync(It.IsAny<string>())).ReturnsAsync((LatLng?)null);

        var result = await _service.GenerateShipmentHubsAsync(shipmentId);

        Assert.That(result, Is.Empty);
    }

    // ── UpdateShipmentStatus ──────────────────────────────────────────────────

    [Test]
    public async Task UpdateStatus_ValidTransition_ShouldReturnTrueAndPublishEvent()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.Booked;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);
        _shipmentRepo.Setup(r => r.UpdateAsync(shipment)).ReturnsAsync(true);

        var result = await _service.UpdateShipmentStatusAsync(shipment.Id, "PICKED_UP", "Delhi Hub");

        Assert.That(result, Is.True);
        _publisher.Verify(p => p.Publish("shipment-status-updated", It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task UpdateStatus_InvalidTransition_ShouldReturnFalse()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.Delivered;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);

        var result = await _service.UpdateShipmentStatusAsync(shipment.Id, "BOOKED");

        Assert.That(result, Is.False);
        _publisher.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Test]
    public async Task UpdateStatus_ShipmentNotFound_ShouldReturnFalse()
    {
        _shipmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Shipment?)null);

        var result = await _service.UpdateShipmentStatusAsync(Guid.NewGuid(), "PICKED_UP");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateStatus_DelayedStatus_ShouldSetDelayReason()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.InTransit;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);
        _shipmentRepo.Setup(r => r.UpdateAsync(shipment)).ReturnsAsync(true);

        var result = await _service.UpdateShipmentStatusAsync(shipment.Id, "DELAYED", "Mumbai", "Weather conditions");

        Assert.That(result, Is.True);
        Assert.That(shipment.DelayReason, Is.EqualTo("Weather conditions"));
    }

    // ── BookShipment ──────────────────────────────────────────────────────────

    [Test]
    public async Task BookShipment_DraftStatus_ShouldSetBookedAndReturnTrue()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.Draft;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);
        _shipmentRepo.Setup(r => r.UpdateAsync(shipment)).ReturnsAsync(true);

        var result = await _service.BookShipmentAsync(shipment.Id);

        Assert.That(result, Is.True);
        Assert.That(shipment.Status, Is.EqualTo(ShipmentStatus.Booked));
        _publisher.Verify(p => p.Publish("shipment-status-updated", It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task BookShipment_NonDraftStatus_ShouldReturnFalse()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.Booked;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);

        var result = await _service.BookShipmentAsync(shipment.Id);

        Assert.That(result, Is.False);
    }

    // ── CancelShipment ────────────────────────────────────────────────────────

    [Test]
    public async Task CancelShipment_DraftStatus_ShouldReturnTrue()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.Draft;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);
        _shipmentRepo.Setup(r => r.DeleteAsync(shipment.Id)).ReturnsAsync(true);

        var result = await _service.CancelShipmentAsync(shipment.Id);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CancelShipment_InTransitStatus_ShouldReturnFalse()
    {
        var shipment = BuildShipment(Guid.NewGuid());
        shipment.Status = ShipmentStatus.InTransit;

        _shipmentRepo.Setup(r => r.GetByIdAsync(shipment.Id)).ReturnsAsync(shipment);

        var result = await _service.CancelShipmentAsync(shipment.Id);

        Assert.That(result, Is.False);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CreateShipmentDto BuildValidCreateDto() => new()
    {
        Sender = new AddressDto { Name = "Sender", Street = "MG Road", City = "Delhi", State = "Delhi", ZipCode = "110001" },
        Receiver = new AddressDto { Name = "Receiver", Street = "GT Road", City = "Jalandhar", State = "Punjab", ZipCode = "144001" },
        Package = new PackageDto { Weight = 5, Description = "Electronics" }
    };

    private static Shipment BuildShipment(Guid id) => new()
    {
        Id = id,
        UserId = Guid.NewGuid(),
        Status = ShipmentStatus.Draft,
        CreatedAt = DateTime.UtcNow,
        SenderAddress = new Address { Id = Guid.NewGuid(), Name = "Sender", Street = "MG Road", City = "Delhi", State = "Delhi", ZipCode = "110001" },
        ReceiverAddress = new Address { Id = Guid.NewGuid(), Name = "Receiver", Street = "GT Road", City = "Jalandhar", State = "Punjab", ZipCode = "144001" },
        Package = new Package { Id = Guid.NewGuid(), Weight = 5, Description = "Electronics" }
    };
}
