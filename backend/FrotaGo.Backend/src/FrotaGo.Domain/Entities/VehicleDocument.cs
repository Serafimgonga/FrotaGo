using System;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class VehicleDocument
{
    public Guid Id { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public DocumentType Type { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
