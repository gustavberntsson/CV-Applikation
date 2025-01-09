using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Titel är obligatoriskt.")]
        [StringLength(100, ErrorMessage = " Titel får inte överskrida 100 tecken.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Beskrivning är obligatoriskt")]
        [StringLength(500, ErrorMessage = " Titel får inte överskrida 500 tecken.")]
        public string Description { get; set; }

        public ICollection<User>? Participants { get; set; }
    }
}
