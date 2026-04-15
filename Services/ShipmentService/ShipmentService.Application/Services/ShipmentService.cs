using ShipmentService.Application.DTOs;
using ShipmentService.Application.Repositories;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Enums;
using ShipmentService.Domain.Events;
namespace ShipmentService.Application.Services;

using Shared.Messaging;
public class ShipmentService : IShipmentService
{
  private readonly IShipmentRepository _repository;
  private readonly IRabbitMQPublisher _publisher;
  public ShipmentService(IShipmentRepository repository, IRabbitMQPublisher publisher)
  {
    _repository = repository;
    _publisher = publisher;
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
    _publisher.Publish("shipment-created", new ShipmentCreatedEvent
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
    _publisher.Publish("shipment-status-updated", new ShipmentStatusUpdatedEvent
    {
      ShipmentId = shipment.Id,
      Status = shipment.Status,
      UpdatedAt = DateTime.UtcNow
    });
    return await _repository.UpdateAsync(shipment);
  }

  public async Task<bool> UpdateShipmentStatusAsync(Guid id, string status)
  {
    var shipment = await _repository.GetByIdAsync(id);
    if (shipment == null)
      return false;

    if (!IsValidStatusTransition(shipment.Status, status))
      return false;

    shipment.Status = status;
    _publisher.Publish("shipment-status-updated", new ShipmentStatusUpdatedEvent
    {
      ShipmentId = shipment.Id,
      Status = shipment.Status,
      UpdatedAt = DateTime.UtcNow
    });
    return await _repository.UpdateAsync(shipment);
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
    var validTransitions = new Dictionary<string, List<string>>
        {
            { ShipmentStatus.Draft, new List<string> { ShipmentStatus.Booked } },
            { ShipmentStatus.Booked, new List<string> { ShipmentStatus.PickedUp } },
            { ShipmentStatus.PickedUp, new List<string> { ShipmentStatus.InTransit } },
            { ShipmentStatus.InTransit, new List<string> { ShipmentStatus.OutForDelivery } },
            { ShipmentStatus.OutForDelivery, new List<string> { ShipmentStatus.Delivered } },
            { ShipmentStatus.Delivered, new List<string>() }
        };

    return validTransitions.ContainsKey(currentStatus) &&
           validTransitions[currentStatus].Contains(newStatus);
  }
}
