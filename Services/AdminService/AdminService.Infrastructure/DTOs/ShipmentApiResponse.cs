using System;

namespace AdminService.Infrastructure.DTOs
{
    public class ShipmentApiResponse
    {
        public List<ShipmentResponseDto> Data { get; set; }
        public int Count { get; set; }
    }
}
