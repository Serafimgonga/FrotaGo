using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Helpers;
using FrotaGo.Mobile.Core.Interfaces;
using FrotaGo.Mobile.Core.Models;

namespace FrotaGo.Mobile.Core.Services;

public class LessonService : ILessonService
{
    private readonly IApiClient _apiClient;

    public LessonService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<TodayLessonDto>> GetTodayLessonsAsync(Guid instructorId)
    {
        var result = await _apiClient.GetAsync<List<TodayLessonDto>>($"{ApiEndpoints.Mobile.TodayLessons}?instructorId={instructorId}");
        return result ?? new List<TodayLessonDto>();
    }

    public async Task<Lesson?> GetLessonDetailsAsync(Guid lessonId)
    {
        return await _apiClient.GetAsync<Lesson>(ApiEndpoints.Mobile.LessonDetails(lessonId));
    }

    public async Task<bool> StartLessonAsync(StartLessonRequest request)
    {
        return await _apiClient.PostAsync(ApiEndpoints.Mobile.StartLesson, request);
    }

    public async Task<bool> FinishLessonAsync(FinishLessonRequest request)
    {
        return await _apiClient.PostAsync(ApiEndpoints.Mobile.FinishLesson, request);
    }
}
