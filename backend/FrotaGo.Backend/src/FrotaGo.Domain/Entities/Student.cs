using System;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string IdentityCardNumber { get; set; } = string.Empty; // BI
    public LicenseCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
