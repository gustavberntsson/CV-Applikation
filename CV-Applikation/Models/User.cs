using Microsoft.AspNetCore.Identity;

namespace CV_Applikation.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<Project> OwnedProjects { get; set; }

        // Many-to-many relationship with projects
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }

    }
}
