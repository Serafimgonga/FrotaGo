namespace FrotaGo.Mobile.Core.DTOs;

public class StartLessonRequest
{
    public Guid LessonId { get; set; }
    public int InitialKm { get; set; }
    public double InitialLatitude { get; set; }
    public double InitialLongitude { get; set; }
}
