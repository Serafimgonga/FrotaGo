using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Helpers;
using FrotaGo.Mobile.Core.Interfaces;
using FrotaGo.Mobile.Core.Models;

namespace FrotaGo.Mobile.Core.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly IStorageService _storageService;

    public InstructorProfile? CurrentProfile { get; private set; }

    public AuthService(IApiClient apiClient, IStorageService storageService)
    {
        _apiClient = apiClient;
        _storageService = storageService;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>(ApiEndpoints.Auth.Login, request);

        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            await _storageService.SaveTokenAsync(response.Token);
            CurrentProfile = new InstructorProfile
            {
                UserId = response.UserId,
                InstructorId = response.InstructorId,
                SchoolId = response.SchoolId,
                Name = response.Name,
                Email = response.Email,
                SchoolName = response.SchoolName
            };
        }

        return response;
    }

    public async Task LogoutAsync()
    {
        await _storageService.RemoveTokenAsync();
        CurrentProfile = null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _storageService.GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
