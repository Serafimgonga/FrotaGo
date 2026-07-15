using System;
using System.Collections.Generic;
using FrotaGo.Domain.Enums;

namespace FrotaGo.Domain.Entities;

public class TrackingSession
{
    public Guid Id { get; set; }
    
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public Guid? InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
    
    public Guid? LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    
    public TrackingStatus Status { get; set; }
    public string Provider { get; set; } = "mobile"; // "mobile", "hardware"
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    
    public ICollection<VehicleLocation> Locations { get; set; } = new List<VehicleLocation>();
}
