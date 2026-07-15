using System.Net.Http.Json;
using System.Net.Http.Headers;
using FrotaGo.Mobile.Models;
using Microsoft.Maui.Storage;

namespace FrotaGo.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(string baseUrl, string? token = null)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Lesson>> GetLessonsAsync()
    {
        try
        {
            var resp = await _http.GetAsync("api/lessons");
            if (resp.IsSuccessStatusCode)
            {
                var list = await resp.Content.ReadFromJsonAsync<List<Lesson>>();
                if (list != null) return list;
            }
        }
        catch
        {
            // ignore and fallback to placeholder
        }

        // fallback placeholder
        await Task.Delay(50);
        return new List<Lesson>
        {
            new Lesson { Id = 10, StudentName = "João Manuel", Vehicle = "Toyota Corolla (LD-25-45-AA)", StartTime = DateTime.Now.AddMinutes(10), Status = LessonStatus.Scheduled }
        };
    }

    public async Task<bool> StartLessonAsync(int lessonId)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"api/lessons/{lessonId}/start", new { lessonId, status = "InProgress", startedAt = DateTime.UtcNow });
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TrackingSession?> StartTrackingSessionAsync(int lessonId, int vehicleId, int instructorId)
    {
        try
        {
            var payload = new { lessonId, vehicleId, instructorId, provider = "Mobile" };
            var resp = await _http.PostAsJsonAsync("api/tracking/start", payload);
            if (!resp.IsSuccessStatusCode) return null;
            var session = await resp.Content.ReadFromJsonAsync<TrackingSession>();
            return session;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SendLocationAsync(string trackingSessionId, double latitude, double longitude, double? speed)
    {
        try
        {
            var payload = new { trackingSessionId, latitude, longitude, speed };
            var resp = await _http.PostAsJsonAsync("api/tracking/location", payload);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StopTrackingAsync(string trackingSessionId)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"api/tracking/{trackingSessionId}/stop", new { trackingSessionId });
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StopLessonAsync(int lessonId)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"api/lessons/{lessonId}/stop", new { lessonId, status = "Completed", endedAt = DateTime.UtcNow });
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
