using System;
using System.Collections.Generic;

namespace AdminService.Infrastructure.DTOs
{
  public class ReportsDto
  {
    public int TotalShipments { get; set; }
    public int DeliveredShipments { get; set; }
    public int FailedShipments { get; set; }
    public int InTransitShipments { get; set; }
    public decimal DeliveredPercentage { get; set; }
    public List<string> Trends { get; set; }
  }
}
