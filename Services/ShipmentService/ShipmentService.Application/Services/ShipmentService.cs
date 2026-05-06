using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Repositories;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Enums;
using ShipmentService.Domain.Events;
using ShipmentService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace ShipmentService.Application.Services;

using Shared.Messaging;
public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _repository;
    private readonly IShipmentHubRepository _shipmentHubRepository;
    private readonly IGeoRoutingService _geoRoutingService;
    private readonly IHubGeneratorService _hubGeneratorService;
    private readonly IPricingService _pricingService;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<ShipmentService> _logger;

    public ShipmentService(
      IShipmentRepository repository,
      IShipmentHubRepository shipmentHubRepository,
      IGeoRoutingService geoRoutingService,
      IHubGeneratorService hubGeneratorService,
      IPricingService pricingService,
      IRabbitMQPublisher publisher,
      ILogger<ShipmentService> logger)
    {
        _repository = repository;
        _shipmentHubRepository = shipmentHubRepository;
        _geoRoutingService = geoRoutingService;
        _hubGeneratorService = hubGeneratorService;
        _pricingService = pricingService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Guid> CreateShipmentAsync(CreateShipmentDto dto, Guid userId)
    {
        ValidateShipmentDto(dto);

        var senderAddress = MapToAddress(dto.Sender);
        var receiverAddress = MapToAddress(dto.Receiver);
        var package = new Package
        {
            Id = Guid.NewGuid(),
            Weight = dto.Package.Weight,
            Description = dto.Package.Description
        };

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = ShipmentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            SenderAddressId = senderAddress.Id,
            SenderAddress = senderAddress,
            ReceiverAddressId = receiverAddress.Id,
            ReceiverAddress = receiverAddress,
            PackageId = package.Id,
            Package = package
        };

        var shipmentId = await _repository.AddAsync(shipment);

        // Calculate price and generate hubs in one operation (reuses coordinate resolution)
        await GenerateShipmentHubsAsync(shipmentId);

        _publisher.Publish("shipment-created", new Shared.Events.ShipmentCreatedEvent
        {
            ShipmentId = shipment.Id,
            UserId = shipment.UserId,
            Status = shipment.Status,
            CreatedAt = shipment.CreatedAt
        });
        return shipmentId;
    }

    public async Task<ShipmentResponseDto?> GetShipmentByIdAsync(Guid id)
    {
        var shipment = await _repository.GetByIdAsync(id);
        return shipment == null ? null : MapToResponseDto(shipment);
    }

    public async Task<List<ShipmentHubDto>> GetShipmentHubsAsync(Guid shipmentId)
    {
        var hubs = await _shipmentHubRepository.GetByShipmentIdAsync(shipmentId);

        return hubs.Select(hub => new ShipmentHubDto
        {
            Id = hub.Id,
            ShipmentId = hub.ShipmentId,
            Latitude = hub.Latitude,
            Longitude = hub.Longitude,
            Name = hub.Name,
            SequenceNumber = hub.SequenceNumber,
            Status = hub.Status
        }).ToList();
    }

    public async Task<List<ShipmentHubDto>> GenerateShipmentHubsAsync(Guid shipmentId)
    {
        var shipment = await _repository.GetByIdAsync(shipmentId);
        if (shipment == null || shipment.SenderAddress == null || shipment.ReceiverAddress == null)
            return new List<ShipmentHubDto>();

        var origin = await ResolveShipmentCoordinatesAsync(shipment.SenderAddress.Name, shipment.SenderAddress.Street, shipment.SenderAddress.City, shipment.SenderAddress.State, shipment.SenderAddress.ZipCode);
        var destination = await ResolveShipmentCoordinatesAsync(shipment.ReceiverAddress.Name, shipment.ReceiverAddress.Street, shipment.ReceiverAddress.City, shipment.ReceiverAddress.State, shipment.ReceiverAddress.ZipCode);

        if (origin == null || destination == null)
        {
            _logger.LogWarning(
              "Skipping virtual hub generation for shipment {ShipmentId} because coordinates could not be resolved.",
              shipmentId);
            return new List<ShipmentHubDto>();
        }

        // Calculate distance and pricing using resolved coordinates (no redundant geocoding)
        var distanceKm = _geoRoutingService.CalculateDistanceKm(origin, destination);
        shipment.Price = _pricingService.CalculateFare(distanceKm, shipment.Package?.Weight ?? 0);
        _logger.LogInformation("Calculated price ₹{Price} for shipment {ShipmentId} (distance: {DistanceKm} km, weight: {WeightKg} kg)",
            shipment.Price, shipment.Id, distanceKm, shipment.Package?.Weight ?? 0);

        // Persist the calculated price
        await _repository.UpdateAsync(shipment);

        var routePoints = await _geoRoutingService.GetRoutePointsAsync(origin, destination);
        var hubs = await _hubGeneratorService.GenerateHubsAsync(shipmentId, routePoints, 120);

        await _shipmentHubRepository.DeleteByShipmentIdAsync(shipmentId);

        if (hubs.Count > 0)
            await _shipmentHubRepository.AddRangeAsync(hubs);

        var result = hubs.Select(hub => new ShipmentHubDto
        {
            Id = hub.Id,
            ShipmentId = hub.ShipmentId,
            Latitude = hub.Latitude,
            Longitude = hub.Longitude,
            Name = hub.Name,
            SequenceNumber = hub.SequenceNumber,
            Status = hub.Status
        }).ToList();

        _logger.LogInformation(
          "Generated and stored {HubCount} virtual hubs for shipment {ShipmentId}.",
          result.Count,
          shipmentId);

        return result;
    }

    public async Task<List<ShipmentResponseDto>> GetUserShipmentsAsync(Guid userId)
    {
        var shipments = await _repository.GetUserShipmentsAsync(userId);
        return shipments.Select(MapToResponseDto).ToList();
    }

    public async Task<List<ShipmentResponseDto>> GetAllShipmentsAsync()
    {
        var shipments = await _repository.GetAllAsync();
        return shipments.Select(MapToResponseDto).ToList();
    }

    public async Task<bool> BookShipmentAsync(Guid id)
    {
        var shipment = await _repository.GetByIdAsync(id);
        if (shipment == null || shipment.Status != ShipmentStatus.Draft)
            return false;

        shipment.Status = ShipmentStatus.Booked;
        _publisher.Publish("shipment-status-updated", new Shared.Events.ShipmentStatusUpdatedEvent
        {
            ShipmentId = shipment.Id,
            Status = shipment.Status,
            UpdatedAt = DateTime.UtcNow
        });
        return await _repository.UpdateAsync(shipment);
    }

    public async Task<bool> UpdateShipmentStatusAsync(Guid id, string status, string? location = null, string? delayReason = null)
    {
        var shipment = await _repository.GetByIdAsync(id);
        if (shipment == null)
            return false;

        if (!IsValidStatusTransition(shipment.Status, status))
            return false;

        shipment.Status = status.ToUpperInvariant();
        shipment.CurrentLocation = location;
        if (shipment.Status == ShipmentStatus.Delayed)
            shipment.DelayReason = delayReason;
        _publisher.Publish("shipment-status-updated", new Shared.Events.ShipmentStatusUpdatedEvent
        {
            ShipmentId = shipment.Id,
            Status = shipment.Status,
            Location = location,
            DelayReason = shipment.Status == ShipmentStatus.Delayed ? delayReason : null,
            UpdatedAt = DateTime.UtcNow
        });
        return await _repository.UpdateAsync(shipment);
    }

    public async Task<bool> UpdateShipmentHubStatusAsync(Guid hubId, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return await _shipmentHubRepository.UpdateHubStatusAsync(hubId, status.ToUpperInvariant());
    }

    public async Task<bool> CancelShipmentAsync(Guid id)
    {
        var shipment = await _repository.GetByIdAsync(id);
        if (shipment == null)
            return false;

        // Only Draft or Booked shipments can be cancelled
        if (shipment.Status != ShipmentStatus.Draft && shipment.Status != ShipmentStatus.Booked)
            return false;

        return await _repository.DeleteAsync(id);
    }



    private Address MapToAddress(AddressDto dto)

    {
        return new Address
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode
        };
    }

    private ShipmentResponseDto MapToResponseDto(Shipment shipment)
    {
        return new ShipmentResponseDto
        {
            Id = shipment.Id,
            UserId = shipment.UserId,
            Status = shipment.Status,
            CurrentLocation = shipment.CurrentLocation,
            DelayReason = shipment.DelayReason,
            Price = shipment.Price,
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt,
            SenderAddress = shipment.SenderAddress == null ? null : new AddressDto
            {
                Name = shipment.SenderAddress.Name,
                Street = shipment.SenderAddress.Street,
                City = shipment.SenderAddress.City,
                State = shipment.SenderAddress.State,
                ZipCode = shipment.SenderAddress.ZipCode
            },
            ReceiverAddress = shipment.ReceiverAddress == null ? null : new AddressDto
            {
                Name = shipment.ReceiverAddress.Name,
                Street = shipment.ReceiverAddress.Street,
                City = shipment.ReceiverAddress.City,
                State = shipment.ReceiverAddress.State,
                ZipCode = shipment.ReceiverAddress.ZipCode
            },
            Package = shipment.Package == null ? null : new PackageDto
            {
                Weight = shipment.Package.Weight,
                Description = shipment.Package.Description
            }
        };
    }

    private static string BuildAddressQuery(params string[] parts)
    {
        return string.Join(", ", new[]
        {
      parts
    }.SelectMany(values => values).Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private async Task<LatLng?> ResolveShipmentCoordinatesAsync(string name, string street, string city, string state, string zipCode)
    {
        var queries = new[]
        {
            BuildAddressQuery(name, street, city, state, zipCode),
            BuildAddressQuery(city, state),
            BuildAddressQuery(city),
            BuildAddressQuery(name)
        }
        .Where(query => !string.IsNullOrWhiteSpace(query))
        .Distinct()
        .ToList();

        foreach (var query in queries)
        {
            var coordinates = await _geoRoutingService.GetCoordinatesAsync(query);
            if (coordinates != null)
                return coordinates;
        }

        return null;
    }

    private void ValidateShipmentDto(CreateShipmentDto dto)
    {
        if (dto.Sender == null || string.IsNullOrWhiteSpace(dto.Sender.Name))
            throw new ArgumentException("Sender name is required.");

        if (dto.Receiver == null || string.IsNullOrWhiteSpace(dto.Receiver.Name))
            throw new ArgumentException("Receiver name is required.");

        if (dto.Package == null || dto.Package.Weight <= 0)
            throw new ArgumentException("Package weight must be greater than zero.");
    }

    private bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        var current = currentStatus?.ToUpperInvariant();
        var next = newStatus?.ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(next))
            return false;

        // Allow repeated IN_TRANSIT for multiple hub/location updates
        if (current == ShipmentStatus.InTransit && next == ShipmentStatus.InTransit)
            return true;

        // DELAYED can resume to its previous active status
        var activeStatuses = new[] { ShipmentStatus.PickedUp, ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery };
        if (next == ShipmentStatus.Delayed && activeStatuses.Contains(current))
            return true;
        if (current == ShipmentStatus.Delayed && activeStatuses.Contains(next))
            return true;

        var validTransitions = new Dictionary<string, List<string>>
        {
            { ShipmentStatus.Draft, new List<string> { ShipmentStatus.Booked } },
            { ShipmentStatus.Booked, new List<string> { ShipmentStatus.PickedUp } },
            { ShipmentStatus.PickedUp, new List<string> { ShipmentStatus.InTransit } },
            { ShipmentStatus.InTransit, new List<string> { ShipmentStatus.OutForDelivery } },
            { ShipmentStatus.OutForDelivery, new List<string> { ShipmentStatus.Delivered } },
            { ShipmentStatus.Delivered, new List<string>() }
        };

        return validTransitions.ContainsKey(current) &&
               validTransitions[current].Contains(next);
    }
}
