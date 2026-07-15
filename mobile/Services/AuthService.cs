using System.Net.Http.Json;
using Microsoft.Maui.Storage;

namespace FrotaGo.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var payload = new { email, password };
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", payload);
            if (!resp.IsSuccessStatusCode) return null;
            var dto = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (dto?.Token is null) return null;
            // store token (replace with SecureStorage for production)
            Preferences.Set("frotago_auth_token", dto.Token);
            return dto.Token;
        }
        catch
        {
            return null;
        }
    }

    private class LoginResponse
    {
        public string? Token { get; set; }
    }
}
