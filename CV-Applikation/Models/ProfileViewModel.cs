namespace CV_Applikation.Models
{
    public class ProfileViewModel
    {
        public string ProfileName { get; set; }

        public Project? Projects { get; set; }
        public List<CV> Cvs { get; set; } = new List<CV>();
    }
}
