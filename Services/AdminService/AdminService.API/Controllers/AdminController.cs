using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminService.Application.Interfaces;
using AdminService.Infrastructure.DTOs;

namespace AdminService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(await _service.GetDashboard(token));
        }

        [HttpGet("shipments/exceptions")]
        public async Task<IActionResult> GetExceptionShipments()
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(await _service.GetExceptionShipments(token));
        }

        [HttpGet("shipments")]
        public async Task<IActionResult> Shipments()
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(await _service.GetAllShipments(token));
        }

        [HttpPut("shipments/{id}/resolve")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            var token = Request.Headers["Authorization"].ToString();

            await _service.ResolveShipment(id, token);

            return Ok("Shipment resolved");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(await _service.GetAllUsers(token));
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto updateDto)
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(await _service.UpdateUserRole(id, updateDto, token));
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(await _service.GetReports(token));
        }
    }
}
