namespace FrotaGo.Mobile.Core.Models;

public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Ligeiro";
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int CompletedPracticalLessons { get; set; }
    public int TotalRequiredLessons { get; set; } = 30;
}
