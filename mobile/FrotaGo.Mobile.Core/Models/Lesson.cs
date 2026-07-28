using FrotaGo.Mobile.Core.Enums;

namespace FrotaGo.Mobile.Core.Models;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public LessonStatus Status { get; set; }
    public int? InitialKm { get; set; }
    public int? FinalKm { get; set; }
    public string? Notes { get; set; }
}
