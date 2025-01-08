using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Languages
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Språk är obligatoriskt.")]
        public string LanguageName { get; set; }

        [Required(ErrorMessage = "Nivå för språk är obligatoriskt.")]
        [Range(1, 5, ErrorMessage = "Nivå måste vara mellan 1 och 5.")] //c
        public int Level { get; set; }
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
