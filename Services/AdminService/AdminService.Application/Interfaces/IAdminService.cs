using System;
using AdminService.Domain.Models;

namespace AdminService.Application.Interfaces
{
    public interface IAdminService
    {
        Task<DashboardMetrics> GetDashboard(string token);

        Task<string> GetAllShipments(string token);

        Task<string> GetShipmentHubs(Guid shipmentId, string token);

        Task<string> GenerateShipmentHubs(Guid shipmentId, string token);

        Task ResolveShipment(Guid shipmentId, string token);

        Task UpdateShipmentHubStatus(Guid hubId, string status, string token);

        Task<string> GetAllUsers(string token);

        Task<string> UpdateUserRole(Guid userId, dynamic updateDto, string token);

        Task<dynamic> GetReports(string token);

        Task<string> GetExceptionShipments(string token);
    }
}
