using Microsoft.AspNetCore.Identity;

namespace CV_Applikation.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<Project> OwnedProjects { get; set; }
        public string? ImageUrl { get; set; }
        
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }

        public bool IsPrivate { get; set; } = false; // Default: offentlig profil
        public bool IsEnabled { get; set; } = true;

    }
}
