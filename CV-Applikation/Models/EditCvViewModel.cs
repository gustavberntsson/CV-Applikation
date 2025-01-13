using CV_Applikation.Models;
using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class EditCvViewModel
    {
        public int CVId { get; set; }

        [Required(ErrorMessage = "Namn för CV är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "CV-namnet får inte överskrida 100 tecken.")]
        public string CVName { get; set; }
        public List<Education> Educations { get; set; } = new List<Education>();
        public List<Languages> Languages { get; set; } = new List<Languages>();
        public List<Skills> Skills { get; set; } = new List<Skills>();
        public List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

        public IFormFile? ImagePath { get; set; }

        public Boolean IsPrivate { get; set;} 

    }
}
