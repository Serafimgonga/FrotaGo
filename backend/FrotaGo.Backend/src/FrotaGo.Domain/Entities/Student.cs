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
    
    // Esteira de Progresso do Aluno
    public bool DocumentsSubmitted { get; set; } = false;
    public bool RegistrationFeePaid { get; set; } = false;
    public bool TheoryCompleted { get; set; } = false;
    public bool PracticalLessonsStarted { get; set; } = false;
    public bool ExamScheduled { get; set; } = false;
    public StudentProgressStatus ProgressStatus { get; set; } = StudentProgressStatus.Registered;
    
    public int RequiredLessonsCount { get; set; } = 20;
    public int CompletedLessonsCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
