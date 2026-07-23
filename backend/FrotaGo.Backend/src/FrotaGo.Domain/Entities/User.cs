using System;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; } // Masculino / Feminino
    public string? IdentityCardNumber { get; set; } // BI / NIF do utilizador
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    // Multi-Tenancy
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }

    // Invitation & Status
    public bool IsActive { get; set; } = true;
    public string? InvitationToken { get; set; }
    public DateTime? InvitationExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
