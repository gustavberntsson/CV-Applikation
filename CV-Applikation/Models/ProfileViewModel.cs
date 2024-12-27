namespace CV_Applikation.Models
{
    public class ProfileViewModel
    {
        public string ProfileName { get; set; }

        public List<Project> Projects { get; set; } = new List<Project>();
        //public Project? Projects { get; set; }
        public List<CV> Cvs { get; set; } = new List<CV>();
    }
}
