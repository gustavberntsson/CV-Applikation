namespace CV_Applikation.Models
{
    public class ProfileViewModel
    {
        public string ProfileName { get; set; }
        public string? ImageUrl { get; set; }
        public string CurrentUserId { get; set; }
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<CV> Cvs { get; set; } = new List<CV>();
    }
}
