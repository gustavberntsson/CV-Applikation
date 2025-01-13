using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Education
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Utbildning är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Skolnamnet får inte överskrida 100 tecken.")]
        public string School { get; set; }

        [Required(ErrorMessage = "Examensform för utbildningen är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Examensform får inte överskrida 100 tecken.")]
        public string Degree { get; set; }

        [Required(ErrorMessage = "Ämnesområde för utbildningen är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Ämnesområde får inte överskrida 100 tecken.")]
        public string FieldOfStudy { get; set; }

        [Required(ErrorMessage = "Startdatum för utbildningen är obligatoriskt.")]
        [Validation.StartDateValidator(ErrorMessage = "Startdatum kan inte vara i framtiden.")]
        public string StartDate { get; set; }
        [Validation.DateGreaterThanValidator("StartDate", ErrorMessage = "Slutdatum behöver vara senare än startdatumet.")]
        public string?EndDate { get; set; }
        
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv {  get; set; }
    }
}
