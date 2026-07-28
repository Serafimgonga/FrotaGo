using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Models;

namespace FrotaGo.Mobile.Core.Interfaces;

public interface ILessonService
{
    Task<List<TodayLessonDto>> GetTodayLessonsAsync(Guid instructorId);
    Task<Lesson?> GetLessonDetailsAsync(Guid lessonId);
    Task<bool> StartLessonAsync(StartLessonRequest request);
    Task<bool> FinishLessonAsync(FinishLessonRequest request);
}
