using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class CV
    {
        [Key]
        public int CVId { get; set; }
        public string CVName { get; set; }
        //public string OwnerId { get; set; } email? eller bara user och kan ta bort?
        public Boolean IsPrivate { get; set; }

        public string UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        public virtual List<Education> Educations { get; set; } = new List<Education>();
        public virtual List<Skills> Skills { get; set; } = new List<Skills>();
        public virtual List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

        public virtual List<Languages> Languages { get; set; } = new List<Languages>();
    }
}
