using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Education
    {
        public string School { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv {  get; set; }
    }
}
