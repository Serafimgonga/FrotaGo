namespace FrotaGo.Mobile.Core.DTOs;

public class GpsLocationDto
{
    public Guid SessionId { get; set; }
    public Guid VehicleId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double SpeedKmh { get; set; }
    public double Heading { get; set; }
    public DateTime Timestamp { get; set; }
}
