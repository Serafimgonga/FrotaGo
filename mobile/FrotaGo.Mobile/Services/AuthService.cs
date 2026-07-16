using System.Net.Http.Json;
using Microsoft.Maui.Storage;

namespace FrotaGo.Mobile.Services;

public class AuthService
{
    private const string TokenKey = "frotago_auth_token";
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
            // Armazenar token de forma segura (encriptado pelo SO)
            await SecureStorage.Default.SetAsync(TokenKey, dto.Token);
            return dto.Token;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<string?> GetTokenAsync()
    {
        try { return await SecureStorage.Default.GetAsync("frotago_auth_token"); }
        catch { return null; }
    }

    public static void Logout()
    {
        SecureStorage.Default.Remove("frotago_auth_token");
    }

    private class LoginResponse
    {
        public string? Token { get; set; }
    }
}
