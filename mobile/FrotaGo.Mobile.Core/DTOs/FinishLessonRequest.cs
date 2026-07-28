namespace FrotaGo.Mobile.Core.DTOs;

public class FinishLessonRequest
{
    public Guid LessonId { get; set; }
    public int FinalKm { get; set; }
    public string? Notes { get; set; }
    public int EvaluationRating { get; set; } = 5;
}
