using ShipmentService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ShipmentService.Infrastructure.Services;

/// <summary>
/// Service for calculating shipment fare based on distance and weight
/// Pricing model: BasePrice + (DistanceKm * PricePerKm) + (WeightKg * PricePerKg)
/// </summary>
public class PricingService : IPricingService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<PricingService> _logger;

  public PricingService(IConfiguration configuration, ILogger<PricingService> logger)
  {
    _configuration = configuration;
    _logger = logger;
  }

  public decimal CalculateFare(double distanceKm, double weightKg)
  {
    try
    {
      // Get pricing configuration
      var basePrice = GetConfigValue("Pricing:BasePrice", 50m);
      var pricePerKm = GetConfigValue("Pricing:PricePerKm", 5m);
      var pricePerKg = GetConfigValue("Pricing:PricePerKg", 10m);
      var minPrice = GetConfigValue("Pricing:MinimumPrice", 50m);
      var maxPrice = GetConfigValue("Pricing:MaximumPrice", 10000m);

      // Calculate fare: Base + Distance charge + Weight charge
      var fare = basePrice + (decimal)distanceKm * pricePerKm + (decimal)weightKg * pricePerKg;

      // Apply min and max limits
      fare = Math.Max(fare, minPrice);
      fare = Math.Min(fare, maxPrice);

      _logger.LogInformation(
          "Calculated fare: ₹{Fare} for distance {Distance} km and weight {Weight} kg",
          fare, distanceKm, weightKg);

      return fare;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error calculating fare for distance {Distance} km and weight {Weight} kg",
          distanceKm, weightKg);
      throw;
    }
  }

  private decimal GetConfigValue(string key, decimal defaultValue)
  {
    var value = _configuration[$"{key}"];
    if (string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value, out var result))
    {
      _logger.LogWarning("Configuration key '{Key}' not found or invalid. Using default value: {DefaultValue}", key, defaultValue);
      return defaultValue;
    }

    return result;
  }
}
