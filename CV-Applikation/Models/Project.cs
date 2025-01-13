using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Titel är obligatoriskt.")]
        [StringLength(100, ErrorMessage = "Titel får inte vara över 100 tecken.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Beskrivning är obligatoriskt.")]
        [StringLength(1000, ErrorMessage = "Beskrivning får inte vara över 1000 tecken.")]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string? OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))] 
        public virtual User? Owner { get; set; }

        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        [NotMapped]
        public List<string> ParticipantIds { get; set; } = new List<string>();
    }
}
