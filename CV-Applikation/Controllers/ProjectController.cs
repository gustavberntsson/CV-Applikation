using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
    public class ProjectController : Controller
    {
        private UserContext context;
        private UserManager<User> userManager;
        public ProjectController(UserContext service, UserManager<User> userManagerr) 
        {
            userManager = userManagerr;
            context = service;

        }

        public ActionResult AddProject()
        {
            var users = context.Users.ToList(); // Hämta alla användare
            ViewBag.Users = users; // Skicka användarna till vyn
            Project project = new Project();
            return View(project);
        }
        [HttpPost]
        public async Task<ActionResult> AddProject(Project project, List<string> participantIds)
        {
            var currentUser = await userManager.GetUserAsync(User);
            project.OwnerId = currentUser.Id;
            project.CreatedAt = DateTime.UtcNow; // Tilldela tid och datum

            var projectUsers = new List<ProjectUser>
        {
            new ProjectUser
        {
            ProjectId = project.ProjectId,
            UserId = currentUser.Id,
            JoinedAt = DateTime.UtcNow,
            Role = "Owner"
        }
    };

            foreach (var participantId in participantIds)
            {
                if (participantId != currentUser.Id)
                {
                    projectUsers.Add(new ProjectUser
                    {
                        ProjectId = project.ProjectId,
                        UserId = participantId,
                        JoinedAt = DateTime.UtcNow,
                        Role = "Participant"
                    });
                }
            }

            project.ProjectUsers = projectUsers;

            context.Projects.Add(project);
            await context.SaveChangesAsync();
            // Hämta den nuvarande användaren
            //var currentUser = await userManager.GetUserAsync(User);  // Använd UserManager för att hämta användaren
            //project.OwnerId = currentUser.Id; // Sätt ägaren av projektet
            //project.CreatedAt = DateTime.UtcNow;

            //// Lägg till den aktuella användaren i ProjectUsers (som deltagare)
            //var projectUsers = new List<ProjectUser>
            //{
            //        new ProjectUser
            //        {
            //            ProjectId = project.ProjectId,
            //            UserId = currentUser.Id, // Ägaren läggs till som deltagare
            //            JoinedAt = DateTime.UtcNow,
            //            Role = "Owner" // Sätt rollen till "Owner" för den aktuella användaren
            //        }
            //};

            //// Lägg till de andra deltagarna om några är valda (utöver ägaren)
            //    foreach (var participantId in participantIds)
            //    {
            //        // Kontrollera så att den valda användaren inte är samma som den som skapade projektet
            //        if (participantId != currentUser.Id)
            //        {
            //            projectUsers.Add(new ProjectUser
            //            {
            //                ProjectId = project.ProjectId,
            //                UserId = participantId,
            //                JoinedAt = DateTime.UtcNow,
            //                Role = "Participant" // De övriga användarna får rollen "Participant"
            //            });
            //        }
            //    }

            //// Lägg till ProjectUser-instans i projektet
            //project.ProjectUsers = projectUsers;

            //// Lägg till projektet i databasen
            //context.Projects.Add(project);
            //await context.SaveChangesAsync();

            //    // Hämta den nuvarande användaren
            //    var currentUser = await userManager.GetUserAsync(User);  // Använd UserManager för att hämta användaren
            //    project.OwnerId = currentUser.Id; // Sätt ägaren av projektet

            //    // Lägg till den aktuella användaren i ProjectUsers (som deltagare)
            //    var projectUsers = new List<ProjectUser>
            //{
            //    new ProjectUser
            //    {
            //        ProjectId = project.ProjectId,
            //        UserId = currentUser.Id, // Ägaren läggs till som deltagare
            //        JoinedAt = DateTime.UtcNow,
            //        Role = "Owner" // Sätt rollen till "Owner" för den aktuella användaren
            //    }
            //};

            //    // Lägg till de andra deltagarna om några är valda
            //    foreach (var participantId in participantIds)
            //    {
            //        projectUsers.Add(new ProjectUser
            //        {
            //            ProjectId = project.ProjectId,
            //            UserId = participantId,
            //            JoinedAt = DateTime.UtcNow,
            //            Role = "Participant" // De övriga användarna får rollen "Participant"
            //        });
            //    }

            //    // Lägg till ProjectUser-instans i projektet
            //    project.ProjectUsers = projectUsers;

            //    // Lägg till projektet i databasen
            //    context.Projects.Add(project);
            //    await context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ProjectList()
        {
            var projects = await context.Projects
                .Include(p => p.ProjectUsers)
                .Select(p => new ProjectViewModel
                {
                    ProjectId = p.ProjectId,
                    Title = p.Title,
                    ParticipantCount = p.ProjectUsers.Count()
                })
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> ProjectDetails(int projectId)
        {
            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var projectDetailsViewModel = new ProjectDetailsViewModel
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                Participants = project.ProjectUsers.Select(pu => new ParticipantViewModel
                {
                    UserId = pu.UserId,
                    UserName = pu.UserProject?.UserName ?? "Okänd användare"
                }).ToList()
            };

            return View(projectDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> JoinProject(int projectId)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kontrollera om användaren redan är med i projektet
            var isAlreadyInProject = await context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == currentUser.Id);

            if (!isAlreadyInProject)
            {
                // Lägg till användaren i projektet
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = currentUser.Id,
                    JoinedAt = DateTime.UtcNow,
                    Role = "Participant" // Standardroll för ny deltagare
                };

                context.ProjectUsers.Add(projectUser);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }
        //public IActionResult ProjectDetails(int id)
        //{
        //    //c
        //    var project = context.Projects
        //   .Include(p => p.Participants)
        //   .FirstOrDefault(p => p.ProjectId == id);

        //    if (project == null) return NotFound();

        //    return View(project);
        //}

    }
}
