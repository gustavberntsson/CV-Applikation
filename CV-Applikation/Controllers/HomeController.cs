using System.Diagnostics;
using CV_Applikation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserContext context;

        public HomeController(ILogger<HomeController> logger, UserContext service)
        {
            _logger = logger;
            context = service;
        }

        public IActionResult Index()
        {

            var Cvs = context.CVs
            .Include(cv => cv.User)
            .Include(cv => cv.Educations)
            .Include(cv => cv.Languages)
            .Include(cv => cv.Skills)
            .Take(3)
            .ToList();
            
            // Hämta ett urval av CVs
            //var cvs = context.CVs
            //    .Include(cv => cv.User)
            //    .Take(10) // Begränsa till 10 CVs
            //    .ToList();

            // Hämta senaste projektet
            var lastProject = context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            var model = new HomeViewModel
            {
                CVs = Cvs,
                ProjectLatest = lastProject
            };


            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
