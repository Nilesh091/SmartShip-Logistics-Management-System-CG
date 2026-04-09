using System;
using System.Net.Http.Headers;
using AdminService.Infrastructure.DTOs;

namespace AdminService.Infrastructure.Clients
{
  public class AuthClient
  {
    private readonly HttpClient _http;

    public AuthClient(HttpClient http)
    {
      _http = http;
    }

    public async Task<string> GetAllUsers(string token)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/admin/users");

      request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

      var response = await _http.SendAsync(request);

      response.EnsureSuccessStatusCode();

      return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> UpdateUserRole(Guid userId, UpdateUserRoleDto updateDto, string token)
    {
      var request = new HttpRequestMessage(HttpMethod.Put, $"/api/auth/admin/users/{userId}");

      request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

      var json = System.Text.Json.JsonSerializer.Serialize(updateDto);
      request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

      var response = await _http.SendAsync(request);

      response.EnsureSuccessStatusCode();

      return await response.Content.ReadAsStringAsync();
    }
  }
}
