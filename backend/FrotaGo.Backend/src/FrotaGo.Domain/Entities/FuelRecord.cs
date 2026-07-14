using System;

namespace FrotaGo.Domain.Entities;

public class FuelRecord
{
    public Guid Id { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public decimal Litres { get; set; }
    public decimal CostPerLitre { get; set; }
    public decimal TotalCost { get; set; }
    public int Odometer { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
