using Moq;
using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Application.Services.Implementations;
using TrackingService.Domain.Entities;

namespace TrackingService.Tests.Services;

[TestFixture]
public class TrackingServiceTests
{
    private Mock<ITrackingRepository> _repoMock;
    private TrackingService.Application.Services.Implementations.TrackingService _service;

    [SetUp]
    public void Setup()
    {
        _repoMock = new Mock<ITrackingRepository>();
        _service = new TrackingService.Application.Services.Implementations.TrackingService(_repoMock.Object);
    }

    // ── AddEventAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task AddEvent_WithEmptyId_ShouldAssignNewGuid()
    {
        var evt = new TrackingEvent { Id = Guid.Empty, ShipmentId = Guid.NewGuid(), Status = "IN_TRANSIT" };

        await _service.AddEventAsync(evt);

        Assert.That(evt.Id, Is.Not.EqualTo(Guid.Empty));
        _repoMock.Verify(r => r.AddEventAsync(evt), Times.Once);
    }

    [Test]
    public async Task AddEvent_WithDefaultTimestamp_ShouldSetTimestampToUtcNow()
    {
        var evt = new TrackingEvent { ShipmentId = Guid.NewGuid(), Status = "BOOKED", Timestamp = default };

        await _service.AddEventAsync(evt);

        Assert.That(evt.Timestamp, Is.Not.EqualTo(default(DateTime)));
        Assert.That(evt.Timestamp.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public async Task AddEvent_WithExistingId_ShouldNotOverwriteId()
    {
        var existingId = Guid.NewGuid();
        var evt = new TrackingEvent { Id = existingId, ShipmentId = Guid.NewGuid(), Status = "DELIVERED" };

        await _service.AddEventAsync(evt);

        Assert.That(evt.Id, Is.EqualTo(existingId));
    }

    // ── GetTrackingAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task GetTracking_ShouldReturnAllEventsForShipment()
    {
        var shipmentId = Guid.NewGuid();
        var events = new List<TrackingEvent>
        {
            new() { Id = Guid.NewGuid(), ShipmentId = shipmentId, Status = "BOOKED" },
            new() { Id = Guid.NewGuid(), ShipmentId = shipmentId, Status = "IN_TRANSIT" }
        };

        _repoMock.Setup(r => r.GetTrackingByShipmentIdAsync(shipmentId)).ReturnsAsync(events);

        var result = await _service.GetTrackingAsync(shipmentId);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Status, Is.EqualTo("BOOKED"));
        Assert.That(result[1].Status, Is.EqualTo("IN_TRANSIT"));
    }

    [Test]
    public async Task GetTracking_NoEvents_ShouldReturnEmptyList()
    {
        var shipmentId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetTrackingByShipmentIdAsync(shipmentId)).ReturnsAsync(new List<TrackingEvent>());

        var result = await _service.GetTrackingAsync(shipmentId);

        Assert.That(result, Is.Empty);
    }

    // ── UpdateShipmentStatusAsync ─────────────────────────────────────────────

    [Test]
    public async Task UpdateShipmentStatus_ShouldAddTrackingEventWithCorrectFields()
    {
        var shipmentId = Guid.NewGuid();

        await _service.UpdateShipmentStatusAsync(shipmentId, "IN_TRANSIT", "Delhi Hub");

        _repoMock.Verify(r => r.AddEventAsync(It.Is<TrackingEvent>(e =>
            e.ShipmentId == shipmentId &&
            e.Status == "IN_TRANSIT" &&
            e.Location == "Delhi Hub" &&
            e.Id != Guid.Empty
        )), Times.Once);
    }

    [Test]
    public async Task UpdateShipmentStatus_DelayedStatus_ShouldSetDelayReason()
    {
        var shipmentId = Guid.NewGuid();

        await _service.UpdateShipmentStatusAsync(shipmentId, "DELAYED", "Mumbai", "Weather conditions");

        _repoMock.Verify(r => r.AddEventAsync(It.Is<TrackingEvent>(e =>
            e.Status == "DELAYED" &&
            e.DelayReason == "Weather conditions"
        )), Times.Once);
    }

    [Test]
    public async Task UpdateShipmentStatus_NonDelayedStatus_ShouldNotSetDelayReason()
    {
        var shipmentId = Guid.NewGuid();

        await _service.UpdateShipmentStatusAsync(shipmentId, "IN_TRANSIT", "Delhi", "some reason");

        _repoMock.Verify(r => r.AddEventAsync(It.Is<TrackingEvent>(e =>
            e.Status == "IN_TRANSIT" &&
            e.DelayReason == null
        )), Times.Once);
    }

    [Test]
    public async Task UpdateShipmentStatus_ShouldAlwaysSetTimestamp()
    {
        var shipmentId = Guid.NewGuid();
        TrackingEvent? captured = null;
        _repoMock.Setup(r => r.AddEventAsync(It.IsAny<TrackingEvent>()))
                 .Callback<TrackingEvent>(e => captured = e);

        await _service.UpdateShipmentStatusAsync(shipmentId, "BOOKED");

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Timestamp, Is.Not.EqualTo(default(DateTime)));
    }
}
