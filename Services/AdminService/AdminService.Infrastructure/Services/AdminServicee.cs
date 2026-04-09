using System;
using System.Text.Json;
using AdminService.Application.Interfaces;
using AdminService.Domain.Models;
using AdminService.Infrastructure.Clients;
using AdminService.Infrastructure.DTOs;

namespace AdminService.Infrastructure.Services
{
    public class AdminServicee : IAdminService
    {
        private readonly ShipmentClient _shipmentClient;
        private readonly AuthClient _authClient;

        public AdminServicee(ShipmentClient shipmentClient, AuthClient authClient)
        {
            _shipmentClient = shipmentClient;
            _authClient = authClient;
        }

        public async Task<DashboardMetrics> GetDashboard(string token)
        {
            var json = await _shipmentClient.GetAllShipments(token);

            // Console.WriteLine("RAW JSON: " + json);

            var response = JsonSerializer.Deserialize<ShipmentApiResponse>(json,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

            var shipments = response?.Data ?? new List<ShipmentResponseDto>();

            return new DashboardMetrics
            {
                TotalShipments = shipments.Count,
                Delivered = shipments.Count(s => s.Status == "DELIVERED"),
                InTransit = shipments.Count(s => s.Status == "IN_TRANSIT"),
                OutForDelivery = shipments.Count(s => s.Status == "OUT_FOR_DELIVERY")
                // optional
                // OutForDelivery = shipments.Count(s => s.Status == "OUT_FOR_DELIVERY")
            };
        }

        public async Task<string> GetAllShipments(string token)
        {
            return await _shipmentClient.GetAllShipments(token);
        }

        public async Task ResolveShipment(Guid shipmentId, string token)
        {
            await _shipmentClient.ResolveShipment(shipmentId, token);
        }

        public async Task<string> GetAllUsers(string token)
        {
            return await _authClient.GetAllUsers(token);
        }

        public async Task<string> UpdateUserRole(Guid userId, dynamic updateDto, string token)
        {
            // Convert dynamic to UpdateUserRoleDto
            var dto = new UpdateUserRoleDto();
            if (updateDto is System.Text.Json.JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty("role", out var roleElement))
                {
                    dto.Role = roleElement.GetString();
                }
            }
            else if (updateDto is UpdateUserRoleDto typedDto)
            {
                dto = typedDto;
            }

            return await _authClient.UpdateUserRole(userId, dto, token);
        }

        public async Task<dynamic> GetReports(string token)
        {
            var json = await _shipmentClient.GetAllShipments(token);

            var response = JsonSerializer.Deserialize<ShipmentApiResponse>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var shipments = response?.Data ?? new List<ShipmentResponseDto>();

            var delivered = shipments.Count(s => s.Status == "DELIVERED");
            var failed = shipments.Count(s => s.Status == "FAILED" || s.Status == "CANCELLED");
            var inTransit = shipments.Count(s => s.Status == "IN_TRANSIT" || s.Status == "OUT_FOR_DELIVERY");
            var total = shipments.Count;

            var deliveredPercentage = total > 0 ? Math.Round((decimal)delivered / total * 100, 2) : 0;

            // Generate trends
            var trends = new List<string>();
            if (deliveredPercentage >= 80)
                trends.Add("Excellent delivery rate");
            if (failed > total * 0.1m)
                trends.Add("High failure rate detected");
            if (inTransit > delivered)
                trends.Add("More shipments in transit than delivered");

            var reportsDto = new ReportsDto
            {
                TotalShipments = total,
                DeliveredShipments = delivered,
                FailedShipments = failed,
                InTransitShipments = inTransit,
                DeliveredPercentage = deliveredPercentage,
                Trends = trends
            };

            return reportsDto;
        }

        public async Task<string> GetExceptionShipments(string token)
        {
            var json = await _shipmentClient.GetAllShipments(token);

            var response = JsonSerializer.Deserialize<ShipmentApiResponse>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var shipments = response?.Data ?? new List<ShipmentResponseDto>();

            // Filter problematic shipments
            var exceptions = shipments.Where(s =>
                s.Status == "FAILED" ||
                s.Status == "STUCK" ||
                s.Status == "DELAYED" ||
                s.Status == "CANCELLED" ||
                s.Status == "EXCEPTION"
            ).ToList();

            var exceptionResponse = new
            {
                Success = true,
                Message = "Exception shipments retrieved successfully",
                Data = exceptions,
                Count = exceptions.Count
            };

            return JsonSerializer.Serialize(exceptionResponse);
        }
    }
}
