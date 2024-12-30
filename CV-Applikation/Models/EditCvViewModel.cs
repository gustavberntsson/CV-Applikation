using CV_Applikation.Models;

namespace CV_Applikation.Models
{
    public class EditCvViewModel
    {
        public int CVId { get; set; }
        public string CVName { get; set; }
        public List<Education> Educations { get; set; } = new List<Education>();
        public List<Languages> Languages { get; set; } = new List<Languages>();
        public List<Skills> Skills { get; set; } = new List<Skills>();
        public List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();


    }
}
