namespace FrotaGo.Mobile.Models;

public enum TrackingStatus { Created, Active, LostConnection, Stopped }

public class TrackingSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int LessonId { get; set; }
    public int VehicleId { get; set; }
    public int InstructorId { get; set; }
    public TrackingStatus Status { get; set; } = TrackingStatus.Created;
    public string Provider { get; set; } = "Mobile";
}
