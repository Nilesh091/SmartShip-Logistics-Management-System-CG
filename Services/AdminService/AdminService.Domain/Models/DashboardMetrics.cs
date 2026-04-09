using System;

namespace AdminService.Domain.Models
{
    public class DashboardMetrics
    {
        public int TotalShipments { get; set; }
        public int Delivered { get; set; }
        public int InTransit { get; set; }
        public int OutForDelivery { get; set; }

    }
}
