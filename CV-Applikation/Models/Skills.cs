using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Skills
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Färdighet är obligatoriskt.")]
        public string SkillName { get; set; }
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
