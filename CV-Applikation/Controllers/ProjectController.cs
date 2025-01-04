using CV_Applikation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var users = context.Users
                .Where(u => u.Id != "GuestId")
                .ToList(); // Hämta alla användare
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

        //public async Task<IActionResult> ProjectList()
        //{
        //    var currentUser = await userManager.GetUserAsync(User); // Kontrollera om användaren är inloggad
        //    var isUserLoggedIn = currentUser != null;

        //    var projects = await context.Projects
        //        .Include(p => p.ProjectUsers)
        //        .ThenInclude(pu => pu.UserProject) // Ladda in användarens profil
        //        .Select(p => new ProjectViewModel
        //        {
        //            ProjectId = p.ProjectId,
        //            Title = p.Title,
        //            ParticipantCount = p.ProjectUsers
        //                .Count(pu => isUserLoggedIn || !pu.UserProject.IsPrivate), // Filtrera bort privata profiler om användaren inte är inloggad
        //            Participants = p.ProjectUsers
        //                .Where(pu => isUserLoggedIn || !pu.UserProject.IsPrivate) // Filtrera deltagare baserat på inloggning
        //                .Select(pu => new ParticipantViewModel
        //                {
        //                    UserId = pu.UserId,
        //                    UserName = pu.UserProject.UserName,
        //                })
        //                .ToList()
        //        })
        //        .ToListAsync();

        //    return View(projects);
        //}

        public async Task<IActionResult> ProjectDetails(int projectId)
        {
            // Kontrollera om användaren är inloggad
            var currentUser = await userManager.GetUserAsync(User); // Hämta aktuell användare
            var isUserLoggedIn = currentUser != null;

            // Hämta projektet och dess deltagare
            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject) // Ladda in användarens profil
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Bygg upp ViewModel och filtrera deltagare baserat på inloggning
            var projectDetailsViewModel = new ProjectDetailsViewModel
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                Participants = project.ProjectUsers
                    .Select(pu => new ParticipantViewModel
                    {
                        UserId = pu.UserId,
                        UserName = isUserLoggedIn 
                            ? (pu.UserProject?.UserName ?? "Okänd användare") 
                            : (pu.UserProject.IsPrivate ? "Privat användare" : pu.UserProject?.UserName ?? "Okänd användare")
                    })
                    .ToList()
            };

            return View(projectDetailsViewModel);
        }
        //public async Task<IActionResult> ProjectDetails(int projectId)
        //{
        //    var project = await context.Projects
        //        .Include(p => p.ProjectUsers)
        //        .ThenInclude(pu => pu.UserProject)
        //        .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    var projectDetailsViewModel = new ProjectDetailsViewModel
        //    {
        //        ProjectId = project.ProjectId,
        //        Title = project.Title,
        //        Description = project.Description,
        //        CreatedAt = project.CreatedAt,
        //        Participants = project.ProjectUsers.Select(pu => new ParticipantViewModel
        //        {
        //            UserId = pu.UserId,
        //            UserName = pu.UserProject?.UserName ?? "Okänd användare"
        //        }).ToList()
        //    };

        //    return View(projectDetailsViewModel);
        //}

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProject(int projectId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await context.Projects
           .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.OwnerId == currentUserId);

            if (project == null)
            {
                return Unauthorized(); // Om användaren inte äger projektet, neka åtkomst
            }

            // Skapa en view model för redigering
            var model = new EditProjectViewModel
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProject(EditProjectViewModel model)
        {
            var currentUserId = await userManager.GetUserAsync(User);

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.OwnerId == currentUserId.Id);

            if (project == null)
            {
                return Unauthorized(); // Säkerställ att användaren äger projektet
            }

            // Uppdatera projektinformationen
            project.Title = model.Title;
            project.Description = model.Description;

            await context.SaveChangesAsync(); // Spara ändringar i databasen

            return RedirectToAction("ProjectDetails", new { projectId = project.ProjectId });
        }

        [Authorize]
        public async Task<IActionResult> MyProjectsToEdit()
        {
            // Hämta inloggad användares ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Hämta projekt som användaren har skapat
            var projects = await context.Projects
                .Where(p => p.OwnerId == userId)
                .Select(p => new ProjectViewModel
                {
                    ProjectId = p.ProjectId,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return View(projects);
        }
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

