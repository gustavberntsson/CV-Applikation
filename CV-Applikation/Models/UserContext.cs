using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Models
{
    public class UserContext : IdentityDbContext<User>
    {

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AboutMe> AboutMes { get; set; }
        public DbSet<ContactInformation> ContactInformation { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Languages> Languages { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Skills> Skills { get; set; }
        public DbSet<WorkExperience> WorkExperiences { get; set; }
        //message inte lagt till än
    }
}
