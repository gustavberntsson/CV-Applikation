using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class ProjectUser
    {
        //[Key]
        public int ProjectId { get; set; }
        //[Key]
        public string UserId { get; set; }

        public DateTime JoinedAt { get; set; }
        public string Role { get; set; } // e.g., "Developer", "Designer", etc.

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? UserProject { get; set; }

        [ForeignKey(nameof(ProjectId))]

        public virtual Project? ProjectIdent { get; set; }
        // Optional: Add additional properties for the relationship
      

    }
}
