using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_Applikation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CV_Applikation.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private UserContext context;
        public AccountController(UserManager<User> userMngr,
        SignInManager<User> signInMngr, UserContext service)
        {
            this.userManager = userMngr;
            this.signInManager = signInMngr;
            context = service;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                User anvandare = new User();
                anvandare.UserName = registerViewModel.AnvandarNamn;
                var result =
                await userManager.CreateAsync(anvandare, registerViewModel.Losenord);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(anvandare, isPersistent: true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(registerViewModel);
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            LoginViewModel loginViewModel = new LoginViewModel();
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                loginViewModel.AnvandarNamn,
                loginViewModel.Losenord,
                isPersistent: loginViewModel.RememberMe,
                lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Fel användarnam/lösenord.");
                }
            }
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ProfileSettings()
        {
            // Hämta den inloggade användaren
            var loggedInUser = await userManager.GetUserAsync(User);

            // Om användaren inte är inloggad, omdirigera till inloggningssidan
            if (loggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Skapa ViewModel för inställningarna
            var model = new ProfileSettingsViewModel
            {
                UserName = loggedInUser.UserName,
                UserID = loggedInUser.Id,
                IsPrivate = loggedInUser.IsPrivate
            };

            // Skicka ViewModel till vyn
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProfileSettings(ProfileSettingsViewModel model)
        {
            var loggedInUser = await userManager.GetUserAsync(User);

            // Om användaren inte är inloggad, omdirigera till inloggningssidan
            if (loggedInUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kontrollera att det är samma användare som försöker uppdatera
            //if (loggedInUser.Id != model.UserID)
            //{
            //    return Unauthorized();
            //}

            // Uppdatera användarens synlighetsinställning
            loggedInUser.IsPrivate = model.IsPrivate;

            // Uppdatera användaren i databasen
            var result = await userManager.UpdateAsync(loggedInUser);

            if (result.Succeeded)
            {
                // Om uppdateringen lyckades, logga in användaren igen (refresh sign-in)
                await signInManager.RefreshSignInAsync(loggedInUser);
                return RedirectToAction("ProfileSettings");
            }

            // Om det uppstod problem, visa felmeddelanden
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Om något gick fel, skicka tillbaka användaren till vyn med felmeddelanden
            return View(model);
        }

        public async Task<IActionResult> Profile(string? UserId = null)
        {
            // If no UserId is provided, get the current user's ID
            // If no UserId is provided, get the current user's ID
            //if (string.IsNullOrEmpty(UserId))
            //{
                var currentUser = await userManager.GetUserAsync(User);
                var isUserLoggedIn = currentUser != null;
            //    if (currentUser == null)
            //    {
            //        return RedirectToAction("Login", "Account");
            //    }
            //    UserId = currentUser.Id;
            //}

            var userEntity = await context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            var userName = userEntity?.UserName ?? "Okänd användare";

            // Hämta användarens CV
            var CVs = await context.CVs
                //.Where(cv => cv.UserId == UserId)
                .Where(cv => cv.UserId == UserId && (isUserLoggedIn || !cv.IsPrivate)) // Visa privata CV:n bara för inloggade
                .Include(cv => cv.User)
                .Include(cv => cv.Educations)
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .Include(cv => cv.WorkExperiences)
                .ToListAsync();

            // Hämta alla projekt som användaren är med i (både som ägare och deltagare)
            var projects = await context.Projects
                .Include(p => p.ProjectUsers) // Inkludera koppling till deltagare
                .ThenInclude(pu => pu.UserProject) // Inkludera användaruppgifter
                .Where(p => p.OwnerId == UserId || p.ProjectUsers.Any(pu => pu.UserId == UserId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var vmodel = new ProfileViewModel
            {
                ProfileName = userName,
                Cvs = CVs,
                Projects = projects
            };

            return View(vmodel);
        }
        //public async Task<IActionResult> Profile(string? UserId = null)
        //{
        //    // If no UserId is provided, get the current user's ID
        //    if (string.IsNullOrEmpty(UserId))
        //    {
        //        var currentUser = await userManager.GetUserAsync(User);
        //        if (currentUser == null)
        //        {
        //            return RedirectToAction("Login", "Account");
        //        }
        //        UserId = currentUser.Id;
        //    }

        //    var userEntity = await context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        //    var userName = userEntity?.UserName ?? "Okänd användare";

        //    var CVs = await context.CVs
        //        .Where(cv => cv.UserId == UserId)
        //        .Include(cv => cv.User)
        //        .Include(cv => cv.Educations)
        //        .Include(cv => cv.Languages)
        //        .Include(cv => cv.Skills)
        //        .Include(cv => cv.WorkExperiences)
        //        .ToListAsync();

        //    var projectss = await context.Projects
        //        .Where(p => p.OwnerId == UserId)
        //        .OrderByDescending(p => p.CreatedAt)
        //        .FirstOrDefaultAsync();

        //    var vmodel = new ProfileViewModel
        //    {
        //        ProfileName = userName,
        //        Cvs = CVs,
        //        Projects = projectss
        //    };

        //    return View(vmodel);
        //}
    }
}
