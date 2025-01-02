using CV_Applikation.Models;

public class ProfileViewModel
{
    public string ProfileName { get; set; }
    public string? ImageUrl { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Adress { get; set; }
    public string? CurrentUserId { get; set; }
    public List<Project> Projects { get; set; } = new List<Project>();
    public List<CV> Cvs { get; set; } = new List<CV>();
}