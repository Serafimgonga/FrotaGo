using System;
using System.Collections.Generic;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class School
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Nome Comercial
    public string ShortName { get; set; } = string.Empty; // Nome Abreviado
    public string Slug { get; set; } = string.Empty; // slug.frotago.ao
    public string NIF { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty; // Número de licença INATRO
    public string LicenseIssuer { get; set; } = "INATRO"; // Entidade emissora
    public DateTime? LicenseIssueDate { get; set; } // Data de emissão
    
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }

    public string Province { get; set; } = "Luanda";
    public string Municipality { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string Plan { get; set; } = "Professional"; // Gratuito, Starter, Professional, Enterprise
    public DateTime? TrialExpiresAt { get; set; } // Expiração do plano Gratuito (30 dias)
    public SchoolStatus Status { get; set; } = SchoolStatus.Pending; // Pending, Approved, Suspended, Rejected
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
}
