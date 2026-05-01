namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Service for calculating shipment pricing based on distance and weight
/// </summary>
public interface IPricingService
{
  /// <summary>
  /// Calculate fare for a shipment based on distance and weight
  /// </summary>
  /// <param name="distanceKm">Distance in kilometers</param>
  /// <param name="weightKg">Weight in kilograms</param>
  /// <returns>Calculated fare price</returns>
  decimal CalculateFare(double distanceKm, double weightKg);
}
