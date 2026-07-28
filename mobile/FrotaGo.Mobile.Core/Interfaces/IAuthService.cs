using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Models;

namespace FrotaGo.Mobile.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    InstructorProfile? CurrentProfile { get; }
}
