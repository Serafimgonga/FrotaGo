using System;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; }
    
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    
    public Guid InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public DateTime ScheduledDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Topic { get; set; } = string.Empty;
    public LessonStatus Status { get; set; }
    public string Observations { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
