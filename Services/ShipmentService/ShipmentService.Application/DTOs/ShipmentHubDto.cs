namespace ShipmentService.Application.DTOs;

public class ShipmentHubDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Name { get; set; }
    public int SequenceNumber { get; set; }
    public string Status { get; set; } = "PENDING";
}
