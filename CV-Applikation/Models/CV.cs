using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class CV
    {
        [Key]
        public int CVId { get; set; }

        [Required(ErrorMessage = "Namn för CV är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "CV-namn får inte vara längre än 100 tecken.")]
        public string CVName { get; set; }

        [Required(ErrorMessage = "Status för CV är obligatoriskt.")]
        public Boolean IsPrivate { get; set; }
        public string? ImagePath { get; set; }

        [Required] //UserId ska alltid vara kopplat till en användare
        public string? UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        public virtual List<Education> Educations { get; set; } = new List<Education>();
        public virtual List<Skills> Skills { get; set; } = new List<Skills>();
        public virtual List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

        public virtual List<Languages> Languages { get; set; } = new List<Languages>();
    }
}
