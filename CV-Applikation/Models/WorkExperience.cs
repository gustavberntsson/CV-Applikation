using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class WorkExperience
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Företagsnamn är obligatoriskt.")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "Arbetsroll är obligatoriskt.")]
        public string Position { get; set; }
        [Required(ErrorMessage = "Arbetsbeskrivning är obligatoriskt.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Startdatum är obligatoriskt.")]
        [Validation.StartDateValidator(ErrorMessage = "Startdatum kan inte vara i framtiden.")]
        public string StartDate { get; set; }
        [Validation.DateGreaterThanValidator("StartDate", ErrorMessage = "Slutdatum behöver vara senare än startdatumet.")]
        public string? EndDate { get; set; }
        public int CVId { get; set; }

        [ForeignKey(nameof(CVId))]
        public virtual CV? Cv { get; set; }
    }
}
