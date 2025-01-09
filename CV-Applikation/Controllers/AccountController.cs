using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_Applikation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using CV_Applikation.Migrations;
using System.Xml;
using System.Xml.Serialization;

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

        public async Task<IActionResult> Profile(string? UserId)
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
            .Select(cv => new CV
            {
                CVId = cv.CVId,
                CVName = cv.CVName,
                ImagePath = cv.ImagePath, // Inkludera ImagePath
                UserId = cv.UserId,
                Educations = cv.Educations,
                Languages = cv.Languages,
                Skills = cv.Skills,
                WorkExperiences = cv.WorkExperiences
            })
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
                ProfileId = userEntity.Id,
                ImageUrl = userEntity?.ImageUrl,
                IsPrivate = userEntity.IsPrivate,
                CurrentUserId = (await userManager.GetUserAsync(User))?.Id,
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
        public async Task<IActionResult> Search(string SearchString)
        {

            if (string.IsNullOrWhiteSpace(SearchString))
            {
                return View("Search", "Home");
            }

            // Hämta användaren baserat på användarnamnet
            var SearchTerms = SearchString.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentUser = await userManager.GetUserAsync(User);
            var isUserLoggedIn = currentUser != null;
            var matchingUsers = await context.Users
    .Where(u =>
        (!u.IsPrivate || isUserLoggedIn) &&
        SearchTerms.All(term =>
            u.UserName.ToLower().Contains(term) || // Kontrollera användarnamn
            context.CVs.Any(cv =>
                cv.UserId == u.Id &&
                (!cv.IsPrivate || isUserLoggedIn) &&
                (
                    cv.Skills.Any(s => s.SkillName.ToLower().Contains(term)) || // Kontrollera färdigheter
                    cv.Educations.Any(e =>
                        e.FieldOfStudy.ToLower().Contains(term) || // Kontrollera studieriktning
                        e.Degree.ToLower().Contains(term)          // Kontrollera examen
                    ) ||
                    cv.WorkExperiences.Any(we =>
                        we.Position.ToLower().Contains(term) ||    // Kontrollera arbetsposition
                        we.Description.ToLower().Contains(term)    // Kontrollera arbetsbeskrivning
                    )
                )
            )
        )
    )
    .ToListAsync();


            if (matchingUsers.Count == 1)
            {
                var user = matchingUsers.First();
                // Hämta kontaktinformation
                var contactInfo = await context.ContactInformation
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

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
                var pmodel = new ProfileViewModel
                {
                    ProfileName = user.UserName,
                    ProfileId = user.Id,
                    ImageUrl = user.ImageUrl,
                    IsPrivate = user.IsPrivate,
                    FirstName = contactInfo?.FirstName ?? "Ej angivet",
                    LastName = contactInfo?.LastName ?? "Ej angivet",
                    Adress = contactInfo?.Adress ?? "Ej angivet",
                    Email = contactInfo?.Email ?? "Ej angivet",
                    PhoneNumber = contactInfo?.PhoneNumber ?? "Ej angivet",
                    Cvs = CVs,
                    Projects = projects,
                    CurrentUserId = currentUser?.Id
                };

                return View("Profile", pmodel);

            }

            var userIds = matchingUsers.Select(u => u.Id).ToList();
            var allCVs = await context.CVs
                .Where(cv => userIds.Contains(cv.UserId) && (isUserLoggedIn || !cv.IsPrivate))
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .Include(cv => cv.Educations)
                .Include(cv => cv.WorkExperiences)
                .ToListAsync();

            var searchResults = matchingUsers.Select(u => new SearchResult
            {
                UserId = u.Id,
                ProfileName = u.UserName,
                ImageUrl = u.ImageUrl,
                Cvs = allCVs.Where(cv => cv.UserId == u.Id).ToList(),
                IsPrivate = u.IsPrivate,
            }).ToList();

            var model = new SearchViewModel
            {
                SearchString = SearchString,
                Results = searchResults
            };

            return View("Search", model);
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
                ProfilePicture = currentUser.ImageUrl,
                Email = kontaktUppgifter?.Email,
                FirstName = kontaktUppgifter?.FirstName,
                LastName = kontaktUppgifter?.LastName,
                Adress = kontaktUppgifter?.Adress,
                PhoneNumber = kontaktUppgifter?.PhoneNumber,
                IsPrivate = currentUser.IsPrivate
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

            currentUser.ImageUrl = model.ProfilePicture;
            kontaktUppgifter.Email = model.Email;
            kontaktUppgifter.FirstName = model.FirstName;
            kontaktUppgifter.LastName = model.LastName;
            kontaktUppgifter.Adress = model.Adress;
            kontaktUppgifter.PhoneNumber = model.PhoneNumber;
            currentUser.IsPrivate = model.IsPrivate;

            await context.SaveChangesAsync();

            return RedirectToAction("Profile");

        }
        [HttpPost]
        public async Task<IActionResult> SparaTillXml(string userId)
        {
            try
            {
                var currentUser = await userManager.FindByIdAsync(userId);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Ingen användare hittades.";
                    return RedirectToAction("Profile");
                }

                var fileName = $"{currentUser.UserName}_data.xml";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                var kontaktUppgifter = await context.ContactInformation
                    .FirstOrDefaultAsync(k => k.UserId == currentUser.Id);

                var cvs = await context.CVs
                    .Where(cv => cv.UserId == currentUser.Id)
                    .Include(cv => cv.Educations)
                    .Include(cv => cv.Languages)
                    .Include(cv => cv.Skills)
                    .Include(cv => cv.WorkExperiences)
                    .ToListAsync();

                var projects = await context.Projects
                    .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.UserProject)
                    .Where(p => p.OwnerId == currentUser.Id || p.ProjectUsers.Any(pu => pu.UserId == currentUser.Id))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var userData = new UserDataXml
                {
                    Profile = new UserProfileXml
                    {
                        UserName = currentUser.UserName,
                        ImageUrl = currentUser.ImageUrl,
                        IsPrivate = currentUser.IsPrivate,
                        ContactInformation = new ContactInformationXml
                        {
                            Email = kontaktUppgifter?.Email,
                            FirstName = kontaktUppgifter?.FirstName,
                            LastName = kontaktUppgifter?.LastName,
                            Adress = kontaktUppgifter?.Adress,
                            PhoneNumber = kontaktUppgifter?.PhoneNumber
                        }
                    },
                    CVs = cvs.Select(cv => new CVXml
                    {
                        CVName = cv.CVName,
                        ImagePath = cv.ImagePath,
                        Educations = cv.Educations?.Select(e => new EducationXml
                        {
                            Degree = e.Degree,
                            FieldOfStudy = e.FieldOfStudy,
                            School = e.School,
                            StartDate = e.StartDate,
                            EndDate = e.EndDate
                        }).ToList(),
                        Languages = cv.Languages?.Select(l => new LanguageXml
                        {
                            LanguageName = l.LanguageName,
                            Level = l.Level
                        }).ToList(),
                        Skills = cv.Skills?.Select(s => new SkillXml
                        {
                            SkillName = s.SkillName
                        }).ToList(),
                        WorkExperiences = cv.WorkExperiences?.Select(w => new WorkExperienceXml
                        {
                            Position = w.Position,
                            CompanyName = w.CompanyName,
                            Description = w.Description,
                            StartDate = w.StartDate,
                            EndDate = w.EndDate
                        }).ToList()
                    }).ToList(),
                    Projects = projects.Select(p => new ProjectXml
                    {
                        Title = p.Title,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        OwnerId = p.OwnerId,
                        ProjectUsers = p.ProjectUsers?.Select(pu => new ProjectUserXml
                        {
                            UserId = pu.UserId,
                            UserName = pu.UserProject?.UserName
                        }).ToList()
                    }).ToList()
                };

                var xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  "
                };

                var serializer = new XmlSerializer(typeof(UserDataXml));
                using (var writer = XmlWriter.Create(filePath, xmlWriterSettings))
                {
                    serializer.Serialize(writer, userData);
                }

                TempData["SuccessMessage"] = $"XML-fil sparad: {fileName}";
                TempData["FilePath"] = filePath;
                return RedirectToAction("Profile", new { userId = currentUser.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ett fel uppstod: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }
    }
}

