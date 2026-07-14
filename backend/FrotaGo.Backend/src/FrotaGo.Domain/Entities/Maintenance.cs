using System;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class Maintenance
{
    public Guid Id { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; }
    public int Odometer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
