using CV_Applikation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var isUserLoggedIn = currentUser != null;
            var currentUserId = currentUser?.Id ?? string.Empty;

            // Hämta användare förutom den inloggade
            var users = context.Users
                .Where(u => u.Id != currentUserId)
                .Where(u => isUserLoggedIn || u.IsPrivate == false)
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

            // Tilldela OwnerId och initialisera ProjectUsers
            project.OwnerId = currentUser.Id;
            project.ProjectUsers ??= new List<ProjectUser>();

            if (!ModelState.IsValid)
            {
                // Logga valideringsfel
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"Property: {modelState.Key}, Error: {error.ErrorMessage}");
                    }
                }

                // Behåll användarlistan vid valideringsfel
                var users = context.Users
                .Where(u => u.Id != currentUser.Id)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                }).ToList();

                ViewBag.Users = new SelectList(users, "Value", "Text");
                //var users = context.Users.Select(u => new SelectListItem
                //{
                //    Value = u.Id,
                //    Text = u.UserName
                //}).ToList();
                //ViewBag.Users = new SelectList(users, "Value", "Text");

                return View(project);
            }

            // Tilldela projektdata och hantera deltagare
            project.CreatedAt = DateTime.UtcNow;

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

            if (project.ParticipantIds != null)
            {
                foreach (var participantId in project.ParticipantIds)
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
            return RedirectToAction("Index", "Home");
        }
        //    [HttpPost]
        //    public async Task<ActionResult> AddProject(Project project, List<string> participantIds)
        //    {
        //        var currentUser = await userManager.GetUserAsync(User);

        //        // Kontrollera om användaren är inloggad
        //        var isUserLoggedIn = currentUser != null;
        //        var currentUserId = currentUser?.Id ?? string.Empty;

        //        // Om ModelState är ogiltig, måste vi fylla ViewBag.Users på nytt
        //        if (ModelState.IsValid)
        //        {



        //            project.OwnerId = currentUser.Id;
        //            project.CreatedAt = DateTime.UtcNow;

        //            var projectUsers = new List<ProjectUser>
        //{
        //    new ProjectUser
        //    {
        //        ProjectId = project.ProjectId,
        //        UserId = currentUser.Id,
        //        JoinedAt = DateTime.UtcNow,
        //        Role = "Owner"
        //    }
        //};

        //            if (participantIds != null)
        //            {
        //                foreach (var participantId in participantIds)
        //                {
        //                    if (participantId != currentUser.Id)
        //                    {
        //                        projectUsers.Add(new ProjectUser
        //                        {
        //                            ProjectId = project.ProjectId,
        //                            UserId = participantId,
        //                            JoinedAt = DateTime.UtcNow,
        //                            Role = "Participant"
        //                        });
        //                    }
        //                }
        //            }

        //            project.ProjectUsers = projectUsers;

        //            context.Projects.Add(project);
        //            await context.SaveChangesAsync();
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            var users = context.Users
        //               .Where(u => u.Id != currentUserId)
        //               .Where(u => isUserLoggedIn || u.IsPrivate == false)
        //               .ToList();
        //            ViewBag.Users = users;

        //            return View(project);

        //        }
        //    }
        //[HttpPost]
        //public async Task<ActionResult> AddProject(Project project, List<string> participantIds)
        //{
        //    var currentUser = await userManager.GetUserAsync(User);
        //    project.OwnerId = currentUser.Id;
        //    if (ModelState.IsValid)
        //    {
        //        //var currentUser = await userManager.GetUserAsync(User);
        //        //project.OwnerId = currentUser.Id;
        //        project.CreatedAt = DateTime.UtcNow; // Tilldela tid och datum

        //        var projectUsers = new List<ProjectUser>
        //        {
        //                new ProjectUser
        //                {
        //                    ProjectId = project.ProjectId,
        //                    UserId = currentUser.Id,
        //                    JoinedAt = DateTime.UtcNow,
        //                    Role = "Owner"
        //                }
        //        };

        //        foreach (var participantId in participantIds)
        //        {
        //            if (participantId != currentUser.Id)
        //            {
        //                projectUsers.Add(new ProjectUser
        //                {
        //                    ProjectId = project.ProjectId,
        //                    UserId = participantId,
        //                    JoinedAt = DateTime.UtcNow,
        //                    Role = "Participant"
        //                });
        //            }
        //        }

        //        project.ProjectUsers = projectUsers;

        //        context.Projects.Add(project);
        //        await context.SaveChangesAsync();
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        return View(project);
        //    }
        //}


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


            if (ModelState.IsValid)
            {

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
            else
            {
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"Property: {modelState.Key}, Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }
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

