using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_Applikation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

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
                User anvandare = new User
                {
                    UserName = registerViewModel.AnvandarNamn,
                    ImageUrl = registerViewModel.ImageUrl,
                };

                ContactInformation kontaktUppgifter = new ContactInformation
                {
                    Email = registerViewModel.Email,
                    FirstName = registerViewModel.FörNamn,
                    LastName = registerViewModel.EfterNamn,
                    Adress = registerViewModel.Adress,
                    PhoneNumber = registerViewModel.TelefonNummer,
                    UserId = anvandare.Id
                };

                var result = await userManager.CreateAsync(anvandare, registerViewModel.Losenord);
                if (result.Succeeded)
                {
                    context.ContactInformation.Add(kontaktUppgifter);
                    await context.SaveChangesAsync();
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
            if (string.IsNullOrEmpty(UserId))
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                UserId = currentUser.Id;
            }

            var userEntity = await context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            var userName = userEntity?.UserName ?? "Okänd användare";

            // Hämta kontaktinformation
            var contactInfo = await context.ContactInformation
                .FirstOrDefaultAsync(c => c.UserId == UserId);

            var CVs = await context.CVs
                .Where(cv => cv.UserId == UserId)
                .Include(cv => cv.User)
                .Include(cv => cv.Educations)
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .Include(cv => cv.WorkExperiences)
                .ToListAsync();

            var projects = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject)
                .Where(p => p.OwnerId == UserId || p.ProjectUsers.Any(pu => pu.UserId == UserId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var vmodel = new ProfileViewModel
            {
                ProfileName = userName,
                ImageUrl = userEntity?.ImageUrl,
                CurrentUserId = userEntity?.UserName,
                FirstName = contactInfo?.FirstName ?? "Ej angivet",
                LastName = contactInfo?.LastName ?? "Ej angivet",
                Adress = contactInfo?.Adress ?? "Ej angivet",
                Email = contactInfo?.Email ?? "Ej angivet",
                PhoneNumber = contactInfo?.PhoneNumber ?? "Ej angivet",
                Cvs = CVs,
                Projects = projects
            };

            return View(vmodel);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string username)
        {
            // Hämta användaren baserat på användarnamnet
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
            var currentUser = await userManager.GetUserAsync(User);
            var isUserLoggedIn = currentUser != null;

            if (user == null)
            {
                // Om användaren inte hittas, visa ett felmeddelande och återgå till föregående vy
                TempData["ErrorMessage"] = "Användaren kunde inte hittas.";
                return RedirectToAction("Index"); // Återgå till en startvy
            }

            // Hämta användarens CV:n
            var CVs = await context.CVs
                .Where(cv => cv.UserId == user.Id && (isUserLoggedIn || !cv.IsPrivate))
                .Include(cv => cv.Educations)
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .Include(cv => cv.WorkExperiences)
                .ToListAsync();

            // Hämta användarens projekt
            var projects = await context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.UserProject)
                .Where(p => p.OwnerId == user.Id || p.ProjectUsers.Any(pu => pu.UserId == user.Id))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Bygg profilmodellen
            var model = new ProfileViewModel
            {
                ProfileName = username,
                Cvs = CVs,
                Projects = projects,
                CurrentUserId = HttpContext.User.Identity?.Name
            };

            // Returnera profilen med rätt vy
            return View("Profile", model); // Notera att vyn heter "ProfilePage"
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Returnerar vyn om modellvalidering misslyckas.
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Ett fel inträffade. Användaren kunde inte hittas.");
                return View(model);
            }

            // Försök ändra lösenord
            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Lösenordet har ändrats framgångsrikt.";
                return RedirectToAction("Profile");
            }

            // Hantera fel från Identity
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var kontaktUppgifter = await context.ContactInformation
        .FirstOrDefaultAsync(k => k.UserId == currentUser.Id);
            
            var model = new EditProfileViewModel
            {
                Email = kontaktUppgifter?.Email,
                FirstName = kontaktUppgifter?.FirstName,
                LastName = kontaktUppgifter?.LastName,
                Adress = kontaktUppgifter?.Adress,
                PhoneNumber = kontaktUppgifter?.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
         

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
;

            var kontaktUppgifter = await context.ContactInformation.FirstOrDefaultAsync(c => c.UserId == currentUser.Id);
            if (kontaktUppgifter == null)
            {
                kontaktUppgifter = new ContactInformation
                {
                    UserId = currentUser.Id
                };
                context.ContactInformation.Add(kontaktUppgifter);
            }

            kontaktUppgifter.Email = model.Email;
            kontaktUppgifter.FirstName = model.FirstName;
            kontaktUppgifter.LastName = model.LastName;
            kontaktUppgifter.Adress = model.Adress;
            kontaktUppgifter.PhoneNumber = model.PhoneNumber;

            await context.SaveChangesAsync();
            
                return RedirectToAction("Profile");

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
