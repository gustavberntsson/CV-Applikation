using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Applikation.Models
{
    public class ProjectUser
    {
       
        public int ProjectId { get; set; }
        
        public string UserId { get; set; }

        public DateTime JoinedAt { get; set; }
        public string Role { get; set; } //"Developer", "Designer", etc.

        [ForeignKey(nameof(UserId))]
        public virtual User? UserProject { get; set; }

        [ForeignKey(nameof(ProjectId))]

        public virtual Project? ProjectIdent { get; set; }
    }
}
