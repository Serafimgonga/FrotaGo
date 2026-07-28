using FrotaGo.Mobile.Core.Enums;

namespace FrotaGo.Mobile.Core.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Category { get; set; } = "Ligeiro";
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public int CurrentMileageKm { get; set; }
}
