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

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> AddProject()
        {
            var current_user = await userManager.GetUserAsync(User);


            //current_userId sätts till det ID som den nuvarande användaren har.
            var current_userId = current_user.Id;
            //Användare hämtas förutom den inloggade, och filtrerar bort inaktiverade användare.
            var users = context.Users
                .Where(u => u.Id != current_userId && u.IsEnabled)
                //En lista med användare skapas (SelectListItem).
                .Select(u => new SelectListItem
                {
                    Value = u.Id,  //ID för användaren blir värdet för listitemen.
                    Text = u.UserName  //Användarnamnet för användaren blir satt som text för listitemen.
                }).ToList();

            //Listan med användare läggs till i ViewBag, för användning i vyer.
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
            var current_user = await userManager.GetUserAsync(User);
            var current_userId = current_user.Id;


            if (!ModelState.IsValid)
            {

                //Vid ogiltig modelstate behåll användarlistan med filtreringen.
                var users = context.Users
                    .Where(u => u.Id != current_userId && u.IsEnabled)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    }).ToList();

                ViewBag.Users = new SelectList(users, "Value", "Text");

                return View(project);
            }

            //Spara projektet om ModelState är giltigt.
            project.OwnerId = current_user.Id;
            project.CreatedAt = DateTime.UtcNow;
            project.ProjectUsers = new List<ProjectUser>
            {
                new ProjectUser
                {
                    ProjectId = project.ProjectId,
                    UserId = current_user.Id,
                    JoinedAt = DateTime.UtcNow,
                    Role = "Owner"
                }
            };

            //Lägga till deltagare i projektet
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
            var current_user = await userManager.GetUserAsync(User);
            //Kollar ifall användaren är inloggad.
            var isUserLoggedIn = current_user != null;

            //Projektet hämtas utifrån projectId
            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound();
            }

            //En vy-modell (ViewModel) skapas för att förse vyn med information. 

            var projectDetailsViewModel = new ProjectDetailsViewModel
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                //Lista över deltagare skapas utifrån ProjectUsers
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
            var current_user = await userManager.GetUserAsync(User);
            if (current_user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Kollar ifall användaren är med i projektet redan
            var isAlreadyInProject = await context.ProjectUsers
                .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == current_user.Id);

            if (!isAlreadyInProject)
            {
                //Användaren läggs till i projektet
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = current_user.Id,
                    JoinedAt = DateTime.UtcNow,
                    Role = "Participant"
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

            //Inloggade användarens ID hämtas.
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //Hämtar projektet 
            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.OwnerId == currentUserId);

            if (project == null)
            {
                return Unauthorized();
            }

            //Alla aktiverade användare hämtas
            var all_users = await context.Users.Where(u => u.IsEnabled).ToListAsync();


            //Användare som redan är med i projektet filtreras bort.
            var currentUserIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();

            var available_users = all_users
                .Where(u => !currentUserIds.Contains(u.Id) && u.Id != project.OwnerId)  //Filtrerar bort projektägaren och deltagare.
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                })
                .ToList();


            //Lista över användare som redan är med i projektet skapas.
            var current_participants = project.ProjectUsers
                .Where(pu => pu.UserId != project.OwnerId && pu.UserProject?.IsEnabled == true) // Projektägaren och inaktiverade användare utesluts.
                .Select(pu => new SelectListItem
                {
                    Value = pu.UserId,
                    Text = pu.UserProject?.UserName ?? pu.UserId
                })
                .ToList();


            var model = new EditProjectViewModel
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                Description = project.Description,
                CurrentParticipants = current_participants,
                AvailableUsers = available_users
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProject(EditProjectViewModel model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.OwnerId == currentUserId);

            if (project == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {

                //Alla användare som är aktiverade hämtas.
                var all_users = await context.Users.Where(u => u.IsEnabled).ToListAsync();


                //Lista skapas över användares ID:n för alla deltagare i projektet.
                var currentUserIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();
                //Användare som redan är med i projektet och projektägaren filtreras bort.
                var available_users = all_users
                    .Where(u => !currentUserIds.Contains(u.Id) && u.Id != project.OwnerId)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    })
                    .ToList();


                //Lista skapas över användare som är med i projektet och uppfyller nedanstående kriterier.
                model.CurrentParticipants = project.ProjectUsers
                    .Where(pu => pu.UserId != project.OwnerId && pu.UserProject?.IsEnabled == true)
                    .Select(pu => new SelectListItem
                    {
                        Value = pu.UserId,
                        Text = pu.UserProject?.UserName ?? pu.UserId
                    }).ToList();

                model.AvailableUsers = available_users;


                return View(model);
            }


            //Projektets titel och beskrivning uppdateras.
            project.Title = model.Title;
            project.Description = model.Description;

            //Nya deltagare läggs till.
            foreach (var userId in model.ParticipantsToAdd)
            {
                if (!project.ProjectUsers.Any(pu => pu.UserId == userId))
                {
                    project.ProjectUsers.Add(new ProjectUser
                    {
                        ProjectId = project.ProjectId,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow,
                        Role = "Participant"
                    });
                }
            }


            //Borttagning av deltagare.
            foreach (var userId in model.ParticipantsToRemove)
            {
                var projectUser = project.ProjectUsers.FirstOrDefault(pu => pu.UserId == userId);
                if (projectUser != null)
                {
                    context.ProjectUsers.Remove(projectUser);
                }
            }


            await context.SaveChangesAsync();
            //Meddelande sätts i TempData som används för att visa användaren bekräftelse.
            TempData["SuccessMessage"] = "Projektet uppdaterades framgångsrikt.";

            //Får användaren att se detaljer för projektet som användaren valde att uppdatera nyss.
            return RedirectToAction("ProjectDetails", new { projectId = project.ProjectId });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProjectsToEdit()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //Projekt hämtas som användaren själv skapat.
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