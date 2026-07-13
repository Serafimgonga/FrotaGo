using System;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty; // Matrícula
    public string Brand { get; set; } = string.Empty;        // Marca
    public string Model { get; set; } = string.Empty;        // Modelo
    public string Chassis { get; set; } = string.Empty;      // Chassis
    public int Year { get; set; }                             // Ano
    public int Odometer { get; set; }                         // Quilometragem
    public FuelType Fuel { get; set; }                        // Combustível
    public TransmissionType Transmission { get; set; }        // Câmbio
    public VehicleStatus Status { get; set; }                 // Estado
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
