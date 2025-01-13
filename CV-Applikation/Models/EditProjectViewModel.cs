using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Titel är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Titel får inte överskrida 100 tecken.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Beskrivning är obligatoriskt.")]
        [StringLength(500, ErrorMessage = "Beskrivning får inte överskrida 500 tecken.")]
        public string Description { get; set; }

        [MinLength(1, ErrorMessage = "Minst en deltagare måste väljas.")]
        public List<SelectListItem> CurrentParticipants { get; set; } = new List<SelectListItem>();

        [MinLength(1, ErrorMessage = "Minst en tillgänglig användare måste finnas.")]
        public List<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();

        public List<string> ParticipantsToAdd { get; set; } = new List<string>();

        public List<string> ParticipantsToRemove { get; set; } = new List<string>();
    }

}
