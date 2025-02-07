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
        private readonly UserContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(UserContext context, UserManager<User> userManager, ILogger<ProjectController> logger)
        {
            // Konstruktor initierar UserContext, UserManager och Logger.
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> AddProject()
        {
            try
            {
                // Hämtar den nuvarande användaren från UserManager.
                var current_user = await _userManager.GetUserAsync(User);

                // Hämtar ID för den nuvarande användaren.
                var current_userId = current_user.Id;
                // Hämtar alla aktiva användare förutom den nuvarande användaren.
                var users = _context.Users
                    .Where(u => u.Id != current_userId && u.IsEnabled)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,  // Användarens ID som värde.
                        Text = u.UserName  // Användarens användarnamn som text.
                    }).ToList();

                // Skapar en SelectList för att skicka till vyn.
                //Kollar ifall listan users har användare.
                //Ifall användare existerar i listan skapas en SelectList utifrån users.
                //Ifall det inte existerar användare blir ViewBag.Users null.
                ViewBag.Users = users.Any() ? new SelectList(users, "Value", "Text") : null;
               
                //Om det finns användare i listan får ViewBag.HasUsers värdet true, blir false annars.
                ViewBag.HasUsers = users.Any();

                Project project = new Project();
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid laddning av sidan för att lägga till projekt");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddProject(Project project)
        {
            try
            {
                // Hämtar den nuvarande användaren.
                var current_user = await _userManager.GetUserAsync(User);
                var current_userId = current_user.Id;

                // Kontrollerar om ModelState är giltig.
                if (!ModelState.IsValid)
                {
                    // Hämtar användare som är aktiva och inte den nuvarande användaren.
                    var users = _context.Users
                        .Where(u => u.Id != current_userId && u.IsEnabled)
                        .Select(u => new SelectListItem
                        {
                            Value = u.Id,
                            Text = u.UserName
                        }).ToList();

                   
                    ViewBag.Users = users.Any() ? new SelectList(users, "Value", "Text") : null;
                    ViewBag.HasUsers = users.Any(); // Kontrollera om det finns några användare.
                    return View(project);
                }

                // Sätter ägare och skapelsedatum för projektet.
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

                // Lägger till deltagare i projektet om de finns angivna.
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

                // Lägger till projektet i databasen.
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid tillägg av projekt");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProjectList()
        {
            try
            {
                // Hämtar en lista av projekt med antal deltagare.
                var projects = await _context.Projects
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av projektlista");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int projectId)
        {
            try
            {
                // Hämtar den nuvarande användaren.
                var current_user = await _userManager.GetUserAsync(User);
                var isUserLoggedIn = current_user != null;

                // Hämtar projektet baserat på dess ID.
                var project = await _context.Projects
                    .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.UserProject)
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId);

                if (project == null)
                {
                    return NotFound();
                }
                bool isUserInProject = current_user != null && project.ProjectUsers.Any(pu => pu.UserId == current_user.Id);
                // Skapar en vy-modell för projektdetaljer.
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
                        .ToList(),
                        IsUserInProject = isUserInProject
                };

                return View(projectDetailsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid visning av projektdetaljer");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> JoinProject(int projectId)
        {
            try
            {
                // Hämtar den nuvarande användaren.
                var current_user = await _userManager.GetUserAsync(User);
                if (current_user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Kontrollerar om användaren redan är med i projektet.
                var isAlreadyInProject = await _context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == projectId && pu.UserId == current_user.Id);

                if (!isAlreadyInProject)
                {
                    // Lägger till användaren som deltagare i projektet.
                    var projectUser = new ProjectUser
                    {
                        ProjectId = projectId,
                        UserId = current_user.Id,
                        JoinedAt = DateTime.UtcNow,
                        Role = "Participant"
                    };

                    _context.ProjectUsers.Add(projectUser);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid anslutning till projekt");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProject(int projectId)
        {
            try
            {
                // Hämtar ID för den nuvarande användaren.
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Hämtar projektet och dess användare.
                var project = await _context.Projects
                    .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.UserProject)
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.OwnerId == currentUserId);

                if (project == null)
                {
                    return Unauthorized();
                }

                // Hämtar alla aktiva användare.
                var all_users = await _context.Users.Where(u => u.IsEnabled).ToListAsync();

                // Skapar en lista över användare som inte är med i projektet.
                var currentUserIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();

                var available_users = all_users
                    .Where(u => !currentUserIds.Contains(u.Id) && u.Id != project.OwnerId)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName
                    })
                    .ToList();

                // Skapar en lista över nuvarande deltagare i projektet.
                var current_participants = project.ProjectUsers
                    .Where(pu => pu.UserId != project.OwnerId && pu.UserProject?.IsEnabled == true)
                    .Select(pu => new SelectListItem
                    {
                        Value = pu.UserId,
                        Text = pu.UserProject?.UserName ?? pu.UserId
                    })
                    .ToList();

                // Skapar modellen för redigering av projektet.
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid redigering av projekt");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProject(EditProjectViewModel model)
        {
            try
            {
                // Hämtar ID för den nuvarande användaren.
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Hämtar projektet baserat på modellens ID.
                var project = await _context.Projects
                    .Include(p => p.ProjectUsers)
                    .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.OwnerId == currentUserId);

                if (project == null)
                {
                    return Unauthorized();
                }

                // Kontrollerar om modellen är giltig.
                if (!ModelState.IsValid)
                {
                    // Hämtar alla aktiva användare.
                    var all_users = await _context.Users.Where(u => u.IsEnabled).ToListAsync();

                    // Skapar en lista över användare som inte är med i projektet.
                    var currentUserIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();
                    var available_users = all_users
                        .Where(u => !currentUserIds.Contains(u.Id) && u.Id != project.OwnerId)
                        .Select(u => new SelectListItem
                        {
                            Value = u.Id,
                            Text = u.UserName
                        })
                        .ToList();

                    // Skapar en lista över nuvarande deltagare i projektet.
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

                // Uppdaterar projektets titel och beskrivning.
                project.Title = model.Title;
                project.Description = model.Description;

                // Lägger till nya deltagare.
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

                // Tar bort deltagare som inte längre ska vara med.
                foreach (var userId in model.ParticipantsToRemove)
                {
                    var projectUser = project.ProjectUsers.FirstOrDefault(pu => pu.UserId == userId);
                    if (projectUser != null)
                    {
                        _context.ProjectUsers.Remove(projectUser);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Projektet uppdaterades framgångsrikt.";
                return RedirectToAction("ProjectDetails", new { projectId = project.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid uppdatering av projekt");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProjectsToEdit()
        {
            try
            {
                // Hämtar ID för den nuvarande användaren.
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Hämtar projekt som skapats av den nuvarande användaren.
                var projects = await _context.Projects
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av mina projekt");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}