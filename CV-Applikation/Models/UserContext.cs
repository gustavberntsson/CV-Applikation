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
        public DbSet<Message> Message { get; set; }

        public DbSet<ProjectUser> ProjectUsers { get; set; }
        //message inte lagt till än

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            // Specifiera att det inte ska finnas någon kaskadborttagning för ProjectId
            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.ProjectIdent)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(pu => pu.ProjectId)
                .OnDelete(DeleteBehavior.Restrict); // NO ACTION kan användas istället för Restrict

            // Specifiera att det inte ska finnas någon kaskadborttagning för UserId
            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.UserProject)
                .WithMany(u => u.ProjectUsers)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Restrict); // NO ACTION kan användas istället för Restrict

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(modelBuilder);
        }
    }
}
