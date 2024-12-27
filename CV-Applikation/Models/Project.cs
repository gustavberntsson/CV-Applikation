using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Koppling till användare (vem som skapade projektet)
        public string OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))] 
        public virtual User? Owner { get; set; }

        // Lista över användare som deltar i projektet
        public virtual ICollection<CV>? Participants { get; set; }
    }
}
