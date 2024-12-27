using CV_Applikation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
    public class ProjectController : Controller
    {
        private UserContext context;
        public ProjectController(UserContext service) 
        {
            context = service;
        }
        public IActionResult ProjectDetails(int id)
        {
            //c
            var project = context.Projects
           .Include(p => p.Participants)
           .FirstOrDefault(p => p.ProjectId == id);

            if (project == null) return NotFound();

            return View(project);
        }

    }
}
