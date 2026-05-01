using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipmentService.Application.Interfaces;
using ShipmentService.Domain.ValueObjects;

namespace ShipmentService.Infrastructure.Services;

public class GeoRoutingService : IGeoRoutingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeoRoutingService> _logger;

    public GeoRoutingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeoRoutingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LatLng?> GetCoordinatesAsync(string place)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Nominatim");
            var url = $"search?q={Uri.EscapeDataString(place)}&format=json&limit=1";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<NominatimResult>>();
            if (results == null || results.Count == 0)
            {
                _logger.LogWarning("No coordinates found for place: {Place}", place);
                return null;
            }

            return new LatLng(double.Parse(results[0].Lat), double.Parse(results[0].Lon));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get coordinates for {Place}", place);
            return null;
        }
    }

    public async Task<List<LatLng>> GetRoutePointsAsync(LatLng origin, LatLng destination)
    {
        try
        {
            var apiKey = _configuration["OpenRouteService:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("OpenRouteService API key is not configured. Route-based hub generation cannot continue.");
                throw new InvalidOperationException("OpenRouteService API key is required to generate virtual hubs from the route.");
            }

            var client = _httpClientFactory.CreateClient("OpenRouteService");
            var body = new
            {
                coordinates = new[]
                {
                    new[] { origin.Lng, origin.Lat },
                    new[] { destination.Lng, destination.Lat }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "v2/directions/driving-car/geojson")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Authorization", apiKey);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var coords = doc.RootElement
                .GetProperty("features")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates")
                .EnumerateArray()
                .Select(c => new LatLng(c[1].GetDouble(), c[0].GetDouble()))
                .ToList();

            return coords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get route points from OpenRouteService");
            throw;
        }
    }

    public async Task<string?> ReverseGeocodeAsync(LatLng point)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Nominatim");
            var url = $"reverse?lat={point.Lat}&lon={point.Lng}&format=json";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var doc = await System.Text.Json.JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());
            var addr = doc.RootElement.GetProperty("address");

            // city → town → village → county → state_district fallback
            foreach (var key in new[] { "city", "town", "village", "county", "state_district" })
            {
                if (addr.TryGetProperty(key, out var val))
                    return val.GetString();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reverse geocode failed for {Lat},{Lng}", point.Lat, point.Lng);
            return null;
        }
    }

    public double CalculateDistanceKm(LatLng origin, LatLng destination)
    {
        const double R = 6371;
        var dLat = ToRad(destination.Lat - origin.Lat);
        var dLon = ToRad(destination.Lng - origin.Lng);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(origin.Lat)) * Math.Cos(ToRad(destination.Lat))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;

    private class NominatimResult
    {
        public string Lat { get; set; } = string.Empty;
        public string Lon { get; set; } = string.Empty;
    }
}
