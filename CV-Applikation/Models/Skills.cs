using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Skills
    {
        public string SkillName { get; set; }
        public int SkillID { get; set; }
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
