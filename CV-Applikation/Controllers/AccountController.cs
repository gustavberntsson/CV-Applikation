using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_Applikation.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Profile(string? UserId = null)
        {
            // If no UserId is provided, get the current user's ID
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

            var CVs = await context.CVs
                .Where(cv => cv.UserId == UserId)
                .Include(cv => cv.User)
                .Include(cv => cv.Educations)
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .Include(cv => cv.WorkExperiences)
                .ToListAsync();

            var projectss = await context.Projects
                .Where(p => p.OwnerId == UserId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var vmodel = new ProfileViewModel
            {
                ProfileName = userName,
                Cvs = CVs,
                Projects = projectss
            };

            return View(vmodel);
        }
    }
}
