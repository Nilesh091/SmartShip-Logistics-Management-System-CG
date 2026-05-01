using System;
using System.Net.Http.Json;
using System.Net.Http.Headers;
namespace AdminService.Infrastructure.Clients
{
    public class ShipmentClient
    {
        private readonly HttpClient _http;

        public ShipmentClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetAllShipments(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/shipments/admin/all");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetShipmentHubs(Guid shipmentId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/shipments/{shipmentId}/hubs");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GenerateShipmentHubs(Guid shipmentId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/shipments/{shipmentId}/hubs/generate");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateShipmentHubStatus(Guid hubId, string status, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/shipments/hubs/{hubId}/status")
            {
                Content = JsonContent.Create(new { status })
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        public async Task ResolveShipment(Guid id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/shipments/{id}/status");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _http.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }
    }
}
