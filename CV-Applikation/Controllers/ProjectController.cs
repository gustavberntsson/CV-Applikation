using CV_Applikation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
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

        //[HttpGet]
        //public ActionResult AddProject()
        //{
        //    var users = context.Users.ToList(); // Hämta alla användare
        //    if (users == null || !users.Any())
        //    {
        //        // Lägg till en fallback här om inga användare hittas
        //        users = new List<User>(); 
        //    }

        //    ViewBag.Users = users; // Skicka användarna till vyn
        //    Project project = new Project();
        //    return View(project);
        //}

        [HttpGet]
        public async Task<ActionResult> AddProject()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id ?? string.Empty;

            // Hämta användare förutom den inloggade, filtrera bort inaktiverade och hantera privata konton
            var users = context.Users
                .Where(u => u.Id != currentUserId && u.IsEnabled) // Exkluderar inaktiverade användare
                .Where(u => currentUser != null || !u.IsPrivate)  // Exkluderar privata konton om användaren inte är inloggad
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                }).ToList();

            ViewBag.Users = new SelectList(users, "Value", "Text");

            Project project = new Project();
            return View(project);
        }
        //[HttpGet]
        //public async Task<ActionResult> AddProject()
        //{
        //    var currentUser = await userManager.GetUserAsync(User);
        //    var isUserLoggedIn = currentUser != null;

        //    var currentUserId = currentUser?.Id ?? string.Empty;
        //    //var users = context.Users.ToList(); // Hämta alla användare
        //    var users = context.Users
        //    .Where(u => u.Id != currentUserId)
        //    .Where(u => isUserLoggedIn || u.IsPrivate == false)
        //    .ToList(); // Hämta alla användare förutom den inloggade
        //    ViewBag.Users = users; // Skicka användarna till vyn
        //    //ViewBag.Users = users; // Skicka användarna till vyn
        //    Project project = new Project();
        //    return View(project);
        //}


        [HttpPost]
        public async Task<ActionResult> AddProject(Project project)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id ?? string.Empty;

            // Om ModelState inte är giltigt, hantera valideringsfel och fyll på användarlistan igen
            if (!ModelState.IsValid)
            {
                // Behåll användarlistan med korrekt filtrering
                var users = context.Users
                    .Where(u => u.Id != currentUserId && u.IsEnabled) // Exkludera inaktiverade användare
                    .Where(u => currentUser != null || !u.IsPrivate)  // Exkludera privata konton för icke-inloggade
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    }).ToList();

                ViewBag.Users = new SelectList(users, "Value", "Text");

                return View(project); // Skicka tillbaka formuläret med valideringsfel
            }

            // Logik för att spara projektet om ModelState är giltigt
            project.OwnerId = currentUser.Id;
            project.CreatedAt = DateTime.UtcNow;
            project.ProjectUsers = new List<ProjectUser>
    {
        new ProjectUser
        {
            ProjectId = project.ProjectId,
            UserId = currentUser.Id,
            JoinedAt = DateTime.UtcNow,
            Role = "Owner"
        }
    };

            // Lägg till deltagare om de finns
            if (project.ParticipantIds != null)
            {
                foreach (var participantId in project.ParticipantIds)
                {
                    project.ProjectUsers.Add(new ProjectUser
                    {
                        ProjectId = project.ProjectId,
                        UserId = participantId,
                        JoinedAt = DateTime.UtcNow,
                        Role = "Participant"
                    });
                }
            }

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
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

        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int projectId)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var isUserLoggedIn = currentUser != null;

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
                Participants = project.ProjectUsers
                    .Select(pu => new ParticipantViewModel
                    {
                        UserId = pu.UserId,
                        UserName = pu.UserProject.IsEnabled
                            ? (isUserLoggedIn || !pu.UserProject.IsPrivate
                                ? pu.UserProject.UserName
                                : "Okänd användare")
                                : "Inaktiverat konto"
                    })
                    .ToList()
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProject(int projectId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject) // Ladda relaterade användare
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.OwnerId == currentUserId);

            if (project == null)
            {
                return Unauthorized(); // Om projektet inte finns eller om användaren inte är ägaren
            }

            // Hämta alla aktiverade användare
            var allUsers = await context.Users.Where(u => u.IsEnabled).ToListAsync();

            // Filtrera bort de användare som redan är med i projektet
            var currentUserIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();

            var availableUsers = allUsers
                .Where(u => !currentUserIds.Contains(u.Id) && u.Id != project.OwnerId) // Utesluter projektägaren
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName // Eller använd någon annan egenskap som FullName om den finns
                })
                .ToList();

            // Filtrera bort de användare som redan är med i projektet och som är inaktiverade
            var currentParticipants = project.ProjectUsers
                .Where(pu => pu.UserId != project.OwnerId && pu.UserProject?.IsEnabled == true) // Utesluter projektägaren och inaktiverade användare
                .Select(pu => new SelectListItem
                {
                    Value = pu.UserId,
                    Text = pu.UserProject?.UserName ?? pu.UserId // Visa användarnamn eller ID om det inte finns
                })
                .ToList();

            // Skapa ViewModel och skicka till vy
            var model = new EditProjectViewModel
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                CurrentParticipants = currentParticipants,
                AvailableUsers = availableUsers
            };

            return View(model); // Återgå till vyn med projektdata och användare
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProject(EditProjectViewModel model, string action)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.OwnerId == currentUserId);

            if (project == null)
            {
                return Unauthorized(); // Om projektet inte finns eller om användaren inte är ägaren
            }

            if (!ModelState.IsValid)
            {
                // Om valideringen misslyckas, fyll på deltagar- och användarlistor för att bevara data
                var allUsers = await context.Users.Where(u => u.IsEnabled).ToListAsync(); // Filtrera bort inaktiverade användare

                // Filtrera bort de användare som redan är med i projektet
                var currentUserIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();

                var availableUsers = allUsers
                    .Where(u => !currentUserIds.Contains(u.Id) && u.Id != project.OwnerId)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    })
                    .ToList();

                model.CurrentParticipants = project.ProjectUsers
                    .Where(pu => pu.UserId != project.OwnerId && pu.UserProject?.IsEnabled == true) // Utesluter inaktiverade användare
                    .Select(pu => new SelectListItem
                    {
                        Value = pu.UserId,
                        Text = pu.UserProject?.UserName ?? pu.UserId
                    }).ToList();

                model.AvailableUsers = availableUsers;

                // Retur till vyn om validering misslyckas
                return View(model);
            }

            // Uppdatera projektets titel och beskrivning
            project.Title = model.Title;
            project.Description = model.Description;

            // Lägg till nya deltagare (bara aktiverade användare)
            foreach (var userId in model.ParticipantsToAdd)
            {
                if (!project.ProjectUsers.Any(pu => pu.UserId == userId))
                {
                    project.ProjectUsers.Add(new ProjectUser
                    {
                        ProjectId = project.ProjectId,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow,
                        Role = "Participant" // Eller annan roll
                    });
                }
            }

            // Ta bort deltagare (bara aktiverade användare)
            foreach (var userId in model.ParticipantsToRemove)
            {
                var projectUser = project.ProjectUsers.FirstOrDefault(pu => pu.UserId == userId);
                if (projectUser != null)
                {
                    context.ProjectUsers.Remove(projectUser);
                }
            }

            // Spara ändringarna i databasen
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Projektet uppdaterades framgångsrikt.";
            return RedirectToAction("ProjectDetails", new { projectId = project.ProjectId });
        }

        [Authorize]
        [HttpGet]
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
}

