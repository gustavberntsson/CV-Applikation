using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Deltagare som redan är kopplade till projektet
        public List<SelectListItem> CurrentParticipants { get; set; } = new List<SelectListItem>();

        // Lista med alla tillgängliga användare för val
        public List<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();

        // Valda deltagare att lägga till
        public List<string> ParticipantsToAdd { get; set; } = new List<string>();

        // Valda deltagare att ta bort
        public List<string> ParticipantsToRemove { get; set; } = new List<string>();
    }
}
