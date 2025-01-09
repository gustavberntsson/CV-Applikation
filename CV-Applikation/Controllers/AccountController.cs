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
        public IActionResult Register() => View();

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
                var loggedInUser = await userManager.GetUserAsync(User);
                if (loggedInUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                UserId = loggedInUser.Id;
            }

            var userEntity = await GetUserAsync(UserId);
            if (userEntity == null)
            {
                return RedirectToAction("Login", "Account"); // Om användaren inte finns, omdirigera till login
            }

            // Bygg ProfileViewModel med hjälp av BuildProfileViewModelAsync
            var currentUser = await userManager.GetUserAsync(User);
            var vmodel = await BuildProfileViewModelAsync(userEntity, currentUser);

            return View(vmodel);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string SearchString)
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                return View("Search", "Home");
            }

            var currentUser = await userManager.GetUserAsync(User);
            var isUserLoggedIn = currentUser != null;

            // Extrahera användare som matchar sökningen
            var matchingUsers = await GetMatchingUsersAsync(SearchString, isUserLoggedIn);

            if (matchingUsers.Count == 1)
            {
                var user = matchingUsers.First();
                // Bygg och returnera profil för en användare
                var profileViewModel = await BuildProfileViewModelAsync(user, currentUser);
                return View("Profile", profileViewModel);
            }

            // Hämta alla CV:n för de matchande användarna
            var allCVs = await GetAllCVsForUsersAsync(matchingUsers, isUserLoggedIn);

            // Skapa sökresultatmodell
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

            var kontaktUppgifter = await GetContactInformationAsync(currentUser.Id);

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
        private async Task UpdateUserProfile(User currentUser, EditProfileViewModel model)
        {
            currentUser.ImageUrl = model.ProfilePicture;
            currentUser.IsPrivate = model.IsPrivate;

            var contactInfo = await context.ContactInformation
                .FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

            if (contactInfo != null)
            {
                contactInfo.Email = model.Email;
                contactInfo.FirstName = model.FirstName;
                contactInfo.LastName = model.LastName;
                contactInfo.Adress = model.Adress;
                contactInfo.PhoneNumber = model.PhoneNumber;
            }

            await context.SaveChangesAsync();
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {


            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            await UpdateUserProfile(currentUser, model);

            return RedirectToAction("Profile");

        }
        [HttpPost]
        public async Task<IActionResult> SparaTillXml(string userId)
        {
            try
            {
                var currentUser = await GetUserAsync(userId);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Ingen användare hittades.";
                    return RedirectToAction("Profile");
                }


                var kontaktUppgifter = await GetContactInformationAsync(currentUser.Id);  // Hämta kontaktinformation
                var cvs = await GetCVsAsync(currentUser.Id);  // Hämta CV:n för användaren
                var projects = await GetProjectsAsync(currentUser.Id);  // Hämta projekt för användaren

                var fileName = $"{currentUser.UserName}_data.xml";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

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
        private async Task<ProfileViewModel> BuildProfileViewModelAsync(User user, User currentUser)
        {
            var contactInfo = await GetContactInformationAsync(user.Id);
            var CVs = await GetCVsAsync(user.Id);
            var projects = await GetProjectsAsync(user.Id);

            return new ProfileViewModel
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
        }
        private async Task<ContactInformation> GetContactInformationAsync(string userId)
        {
            return await context.ContactInformation
            .FirstOrDefaultAsync(c => c.UserId == userId);
        }
        private async Task<User> GetUserAsync(string userId)
        {
            return await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        }
        private async Task<List<User>> GetMatchingUsersAsync(string searchString, bool isUserLoggedIn)
        {
            var searchTerms = searchString.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return await context.Users
                .Where(u =>
                    (!u.IsPrivate || isUserLoggedIn) &&
                    searchTerms.All(term =>
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
        }
        private async Task<List<CV>> GetCVsAsync(string userId)
        {
            return (await context.CVs
            .Where(cv => cv.UserId == userId)
            .Include(cv => cv.Educations)
            .Include(cv => cv.Languages)
            .Include(cv => cv.Skills)
            .Include(cv => cv.WorkExperiences)
            .ToListAsync());
        }
        private async Task<List<CV>> GetAllCVsForUsersAsync(List<User> matchingUsers, bool isUserLoggedIn)
        {
            var userIds = matchingUsers.Select(u => u.Id).ToList();

            return await context.CVs
                .Where(cv => userIds.Contains(cv.UserId) && (isUserLoggedIn || !cv.IsPrivate))
                .Include(cv => cv.Languages)
                .Include(cv => cv.Skills)
                .Include(cv => cv.Educations)
                .Include(cv => cv.WorkExperiences)
                .ToListAsync();
        }
        private async Task<List<Project>> GetProjectsAsync(string userId)
        {
            return await context.Projects
            .Include(p => p.ProjectUsers)
            .ThenInclude(pu => pu.UserProject)
            .Where(p => p.OwnerId == userId || p.ProjectUsers.Any(pu => pu.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        }
    }
}

