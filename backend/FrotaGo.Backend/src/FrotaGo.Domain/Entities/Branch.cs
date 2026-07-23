using System;

namespace FrotaGo.Domain.Entities;

public class Branch
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public School? School { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
