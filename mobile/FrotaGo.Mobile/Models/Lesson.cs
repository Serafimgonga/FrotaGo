namespace FrotaGo.Mobile.Models;

public enum LessonStatus { Scheduled, Started, InProgress, Completed }

public class Lesson
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Vehicle { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public int InstructorId { get; set; }
    public DateTime StartTime { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.Scheduled;
}
