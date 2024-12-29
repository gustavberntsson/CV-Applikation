using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Titel är obligatoriskt")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Beskrivning är obligatoriskt")]
        public string Description { get; set; }
    }
}
