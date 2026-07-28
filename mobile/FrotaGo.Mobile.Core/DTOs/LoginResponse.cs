namespace FrotaGo.Mobile.Core.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid InstructorId { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
