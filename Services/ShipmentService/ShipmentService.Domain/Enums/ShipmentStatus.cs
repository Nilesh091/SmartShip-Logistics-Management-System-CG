namespace ShipmentService.Domain.Enums;

public static class ShipmentStatus
{
  public const string Draft = "DRAFT";

  public const string Booked = "BOOKED";

  public const string PickedUp = "PICKED_UP";

  public const string InTransit = "IN_TRANSIT";

  public const string OutForDelivery = "OUT_FOR_DELIVERY";

  public const string Delivered = "DELIVERED";
}
