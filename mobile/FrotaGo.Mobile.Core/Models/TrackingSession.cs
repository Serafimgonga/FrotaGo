using FrotaGo.Mobile.Core.Enums;

namespace FrotaGo.Mobile.Core.Models;

public class TrackingSession
{
    public Guid SessionId { get; set; }
    public Guid InstructorId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? PracticalLessonId { get; set; }
    public TrackingStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
