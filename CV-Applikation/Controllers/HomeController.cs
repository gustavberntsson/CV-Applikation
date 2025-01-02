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
            // Hämta den aktuella användaren
            var currentUser = await userManager.GetUserAsync(User);

            // Hämta ett urval av CVs
            var Cvs = await context.CVs
                .Include(cv => cv.User)
                .Where(cv => !cv.IsPrivate)
                .Where(cv => cv.User.IsPrivate == false)
                .Include(cv => cv.Educations)
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .ToListAsync();

            // Hämta senaste projektet
            var lastProject = await context.Projects
                .Where(p => p.Owner.IsPrivate == false)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            // Kontrollera om användaren är med i det senaste projektet
            bool isUserInProject = false;

            if (currentUser != null && lastProject != null)
            {
                isUserInProject = await context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == lastProject.ProjectId && pu.UserId == currentUser.Id);
            }

            // Bygg vymodellen
            var model = new HomeViewModel
            {
                CVs = Cvs,
                ProjectLatest = lastProject,
                IsUserInProject = isUserInProject
            };

            return View(model);
        }

        //public IActionResult Index()
        //{

        //    var Cvs = context.CVs
        //    .Include(cv => cv.User)
        //    .Include(cv => cv.Educations)
        //    .Include(cv => cv.Languages)
        //    .Include(cv => cv.Skills)
        //    .Take(3)
        //    .ToList();

        //    // Hämta ett urval av CVs
        //    //var cvs = context.CVs
        //    //    .Include(cv => cv.User)
        //    //    .Take(10) // Begränsa till 10 CVs
        //    //    .ToList();

        //    // Hämta senaste projektet
        //    var lastProject = context.Projects
        //        .OrderByDescending(p => p.CreatedAt)
        //        .FirstOrDefault();

        //    var model = new HomeViewModel
        //    {
        //        CVs = Cvs,
        //        ProjectLatest = lastProject
        //    };


        //    return View(model);
        //}

        public IActionResult Profile()
        {
            var currentUser = userManager.GetUserAsync(User).Result;
            if (currentUser == null)
                return RedirectToAction("Login", "Account");
            return RedirectToAction("Profile", "Account", new { UserId = currentUser.Id });
        }

        public IActionResult Message()
        {
            var currentUser = userManager.GetUserAsync(User).Result;
            if (currentUser == null)
                return RedirectToAction("Login", "Account");
            return RedirectToAction("Message", "Message");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
