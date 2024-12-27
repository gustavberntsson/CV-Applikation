﻿using CV_Applikation.Models;
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
