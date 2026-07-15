using System;

namespace FrotaGo.Domain.Entities;

public class VehicleLocation
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public Guid? TrackingSessionId { get; set; }
    public TrackingSession? TrackingSession { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Speed { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
