using System;

namespace FrotaGo.Domain.Entities;

public enum AccidentSeverity
{
    Leve = 1,
    Moderada = 2,
    Grave = 3
}

public enum AccidentStatus
{
    Pendente = 1,
    EmResolucao = 2,
    Resolvido = 3
}

public class Accident
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public AccidentSeverity Severity { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Location { get; set; } = string.Empty;
    public AccidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
