using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<Project> OwnedProjects { get; set; }
        public string? ImageUrl { get; set; }
        // Many-to-many relationship with projects
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }

        [Required(ErrorMessage = "Privacy status is required.")]
        public bool IsPrivate { get; set; } = false; // Default: public profile
        public bool IsEnabled { get; set; } = true;

    }
}
