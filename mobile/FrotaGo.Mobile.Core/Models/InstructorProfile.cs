namespace FrotaGo.Mobile.Core.Models;

public class InstructorProfile
{
    public Guid UserId { get; set; }
    public Guid InstructorId { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
}
