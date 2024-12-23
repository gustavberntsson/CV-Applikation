using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class WorkExperience
    {
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
