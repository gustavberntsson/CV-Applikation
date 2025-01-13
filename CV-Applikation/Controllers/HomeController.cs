using System.Diagnostics;
using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserContext context;
        private UserManager<User> userManager;

        public HomeController(ILogger<HomeController> logger, UserContext service, UserManager<User> userManagerr)
        {
            _logger = logger;
            context = service;
            userManager = userManagerr;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                //Den aktuella användaren hämtas, kopplat till inloggningen.
                var currentUser = await userManager.GetUserAsync(User);

                //Urval av CVs hämtas som uppfyller särskilda kriterier.
                var Cvs = await context.CVs
                    .Include(cv => cv.User)
                    .Where(cv => !cv.IsPrivate) //Privata CV:n filtreras bort 
                    .Where(cv => cv.User.IsPrivate == false) //CV:n som har en privat ägare filtreras bort.
                    .Where(cv => cv.User.IsEnabled == true) //CV:n som har en inaktiverad ägare filtreras bort.
                    .Include(cv => cv.Educations)
                    .Include(cv => cv.Languages)
                    .Include(cv => cv.Skills)
                    .ToListAsync();

                //Senaste projektet hämtas som följer särskilda kriterier.
                var lastProject = await context.Projects
                    .Where(p => p.Owner.IsPrivate == false)
                    .Where(p => p.Owner.IsEnabled == true)
                    .OrderByDescending(p => p.CreatedAt) //Projekten sorteras efter senaste skapande.
                    .FirstOrDefaultAsync(); //Det senaste (första) projektet hämtas. 

                bool isUserInProject = false;

                if (currentUser != null && lastProject != null)
                {
                    //Kontrollerar ifall det existerar en koppling mellan den aktuella användaren och det senaste projektet.
                    isUserInProject = await context.ProjectUsers
                        .AnyAsync(pu => pu.ProjectId == lastProject.ProjectId && pu.UserId == currentUser.Id);
                }

                var model = new HomeViewModel
                {
                    CVs = Cvs,
                    ProjectLatest = lastProject,
                    IsUserInProject = isUserInProject
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ett fel uppstod: {ex.Message}");
                return View("Error");
            }
        }

        public IActionResult Profile()
        {
            try
            {
                var currentUser = userManager.GetUserAsync(User).Result;
                if (currentUser == null)
                    return RedirectToAction("Login", "Account");
                return RedirectToAction("Profile", "Account", new { UserId = currentUser.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ett fel uppstod vid hämtning av profil: {ex.Message}");
                return RedirectToAction("Error");
            }
        }

        public IActionResult Message()
        {
            try
            {
                var currentUser = userManager.GetUserAsync(User).Result;
                if (currentUser == null)
                    return RedirectToAction("Login", "Account");
                return RedirectToAction("Message", "Message");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ett fel uppstod vid hämtning av meddelanden: {ex.Message}");
                return RedirectToAction("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ett fel uppstod: {ex.Message}");
                return View(new ErrorViewModel { RequestId = "Okänt" });
            }
        }
    }
}
