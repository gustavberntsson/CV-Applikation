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
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userMngr,
        SignInManager<User> signInMngr, UserContext service, ILogger<AccountController> logger)
        {
            this.userManager = userMngr;
            this.signInManager = signInMngr;
            context = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid åtkomst av registreringssidan");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid registrering av användare");
                ModelState.AddModelError("", "Ett fel uppstod vid registrering. Försök igen senare.");
                return View(registerViewModel);
            }
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            try
            {
                LoginViewModel loginViewModel = new LoginViewModel();
                return View(loginViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid åtkomst av inloggningssidan");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByNameAsync(loginViewModel.AnvandarNamn);

                    if (user != null && !user.IsEnabled)
                    {
                        ModelState.AddModelError("", "Detta konto har inaktiverats. Kontakta administratör om du önskar återaktivera kontot.");
                        return View(loginViewModel);
                    }

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
                        ModelState.AddModelError("", "Fel användarnamn/lösenord.");
                    }
                }
                return View(loginViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid inloggning");
                ModelState.AddModelError("", "Ett fel uppstod vid inloggning. Försök igen senare.");
                return View(loginViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid utloggning");
                TempData["ErrorMessage"] = "Ett fel uppstod vid utloggning. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Profile(string? UserId)
        {
            try
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

                if (userEntity == null || !userEntity.IsEnabled)
                {
                    return RedirectToAction("Index", "Home");
                }

                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null || currentUser?.Id != UserId)
                {
                    var contactInfo = await GetContactInformationAsync(UserId);
                    if (contactInfo != null)
                    {
                        contactInfo.ViewCount = (contactInfo.ViewCount ?? 0) + 1;
                        await context.SaveChangesAsync();
                    }
                }

                var vmodel = await BuildProfileViewModelAsync(userEntity, currentUser);
                return View(vmodel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av profil");
                TempData["ErrorMessage"] = "Ett fel uppstod vid hämtning av profilen. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string SearchString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchString))
                {
                    return View("Search", "Home");
                }

                var currentUser = await userManager.GetUserAsync(User);
                var isUserLoggedIn = currentUser != null;

                var matchingUsers = await context.Users
                    .Where(u => u.IsEnabled)
                    .Where(u => (isUserLoggedIn || !u.IsPrivate) &&
                            u.UserName.Contains(SearchString))
                    .ToListAsync();

                if (matchingUsers.Count == 1)
                {
                    var user = matchingUsers.First();
                    if (!isUserLoggedIn || currentUser.Id != user.Id)
                    {
                        var contactInfo = await GetContactInformationAsync(user.Id);
                        if (contactInfo != null)
                        {
                            contactInfo.ViewCount = (contactInfo.ViewCount ?? 0) + 1;
                            await context.SaveChangesAsync();
                        }
                    }
                    var profileViewModel = await BuildProfileViewModelAsync(user, currentUser);
                    return View("Profile", profileViewModel);
                }

                var allCVs = await GetAllCVsForUsersAsync(matchingUsers, isUserLoggedIn);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid sökning");
                TempData["ErrorMessage"] = "Ett fel uppstod vid sökning. Försök igen senare.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            try
            {
                return View(new ChangePasswordViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid åtkomst av lösenordsändringssidan");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Ett fel inträffade. Användaren kunde inte hittas.");
                    return View(model);
                }

                var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Det nuvarande lösenordet är felaktigt.");
                    return View(model);
                }

                var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Lösenordet har ändrats.";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid ändring av lösenord");
                ModelState.AddModelError("", "Ett fel uppstod vid ändring av lösenord. Försök igen senare.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av profilredigeringssidan");
                TempData["ErrorMessage"] = "Ett fel uppstod. Försök igen senare.";
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                await UpdateUserProfile(currentUser, model);
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid uppdatering av profil");
                ModelState.AddModelError("", "Ett fel uppstod vid uppdatering av profilen. Försök igen senare.");
                return View(model);
            }
        }

        private async Task UpdateUserProfile(User currentUser, EditProfileViewModel model)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid uppdatering av användarprofil");
                throw;
            }
        }

        private async Task<ProfileViewModel> BuildProfileViewModelAsync(User user, User currentUser)
        {
            try
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
                    IsEnabled = user.IsEnabled,
                    FirstName = contactInfo?.FirstName ?? "Ej angivet",
                    LastName = contactInfo?.LastName ?? "Ej angivet",
                    Adress = contactInfo?.Adress ?? "Ej angivet",
                    Email = contactInfo?.Email ?? "Ej angivet",
                    PhoneNumber = contactInfo?.PhoneNumber ?? "Ej angivet",
                    ViewCount = contactInfo?.ViewCount ?? 0,
                    Cvs = CVs,
                    Projects = projects,
                    CurrentUserId = currentUser?.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid byggande av profilvy");
                throw;
            }
        }

        private async Task<ContactInformation> GetContactInformationAsync(string userId)
        {
            try
            {
                return await context.ContactInformation
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av kontaktinformation");
                throw;
            }
        }

        private async Task<User> GetUserAsync(string userId)
        {
            try
            {
                return await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av användare");
                throw;
            }
        }

        private async Task<List<User>> GetMatchingUsersAsync(string searchString, bool isUserLoggedIn)
        {
            try
            {
                var searchTerms = searchString.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                return await context.Users
                    .Where(u =>
                        (!u.IsPrivate || isUserLoggedIn) &&
                        searchTerms.All(term =>
                            u.UserName.ToLower().Contains(term) ||
                            context.CVs.Any(cv =>
                                cv.UserId == u.Id &&
                                (!cv.IsPrivate || isUserLoggedIn) &&
                                (
                                    cv.Skills.Any(s => s.SkillName.ToLower().Contains(term)) ||
                                    cv.Educations.Any(e =>
                                        e.FieldOfStudy.ToLower().Contains(term) ||
                                        e.Degree.ToLower().Contains(term)
                                    ) ||
                                    cv.WorkExperiences.Any(we =>
                                        we.Position.ToLower().Contains(term) ||
                                        we.Description.ToLower().Contains(term)
                                    )
                                )
                            )
                        )
                    )
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid sökning efter matchande användare");
                throw;
            }
        }

        private async Task<List<CV>> GetCVsAsync(string userId)
        {
            try
            {
                return await context.CVs
                    .Where(cv => cv.UserId == userId)
                    .Include(cv => cv.Educations)
                    .Include(cv => cv.Languages)
                    .Include(cv => cv.Skills)
                    .Include(cv => cv.WorkExperiences)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av CV:n");
                throw;
            }
        }

        private async Task<List<CV>> GetAllCVsForUsersAsync(List<User> matchingUsers, bool isUserLoggedIn)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av CV:n för användare");
                throw;
            }
        }

        private async Task<List<Project>> GetProjectsAsync(string userId)
        {
            try
            {
                return await context.Projects
                    .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.UserProject)
                    .Where(p => p.OwnerId == userId || p.ProjectUsers.Any(pu => pu.UserId == userId))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av projekt");
                throw;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DisableAccount()
        {
            try
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return NotFound();
                }

                currentUser.IsEnabled = false;
                var result = await userManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    var projectsOwnedByUser = await context.Projects
                        .Where(p => p.OwnerId == currentUser.Id)
                        .ToListAsync();

                    foreach (var project in projectsOwnedByUser)
                    {
                        await HandleDisabledOwner(project.ProjectId);
                    }

                    await signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction("EditProfile");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid inaktivering av konto");
                TempData["ErrorMessage"] = "Ett fel uppstod vid inaktivering av kontot.";
                return RedirectToAction("EditProfile");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindSimilarUser(string userId)
        {
            try
            {
                var currentUser = await GetUserAsync(userId);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Användaren kunde inte hittas.";
                    return RedirectToAction("Profile", new { userId });
                }

                var userSchools = await context.CVs
                    .Where(cv => cv.UserId == userId)
                    .SelectMany(cv => cv.Educations)
                    .Select(e => e.School)
                    .Distinct()
                    .ToListAsync();

                if (!userSchools.Any())
                {
                    TempData["ErrorMessage"] = "Inga skolor hittades för denna användare.";
                    return RedirectToAction("Profile", new { userId });
                }

                var similarUsers = await context.Users
                    .Where(u => u.Id != userId)
                    .Where(u => u.IsEnabled)
                    .Where(u => context.CVs
                        .Where(cv => cv.UserId == u.Id)
                        .SelectMany(cv => cv.Educations)
                        .Any(e => userSchools.Contains(e.School)))
                    .ToListAsync();

                if (!similarUsers.Any())
                {
                    TempData["ErrorMessage"] = "Inga användare från samma skola hittades.";
                    return RedirectToAction("Profile", new { userId });
                }

                var random = new Random();
                var randomUser = similarUsers[random.Next(similarUsers.Count)];

                return RedirectToAction("Profile", new { userId = randomUser.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid sökning efter liknande användare");
                TempData["ErrorMessage"] = "Ett fel uppstod vid sökning efter liknande användare.";
                return RedirectToAction("Profile", new { userId });
            }
        }

        private async Task HandleDisabledOwner(int projectId)
        {
            try
            {
                var project = await context.Projects
                    .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.UserProject)
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId);

                if (project == null) return;

                var owner = await context.Users.FirstOrDefaultAsync(u => u.Id == project.OwnerId);
                if (owner != null && !owner.IsEnabled)
                {
                    var newOwner = project.ProjectUsers
                        .Where(pu => pu.UserProject.IsEnabled)
                        .Select(pu => pu.UserId)
                        .FirstOrDefault();

                    if (newOwner != null)
                    {
                        project.OwnerId = newOwner;
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hantering av inaktiverad projektägare");
                throw;
            }
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

                var kontaktUppgifter = await GetContactInformationAsync(currentUser.Id);
                var cvs = await GetCVsAsync(currentUser.Id);
                var projects = await GetProjectsAsync(currentUser.Id);

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
                _logger.LogError(ex, "Ett fel uppstod vid sparande av XML-fil");
                TempData["ErrorMessage"] = $"Ett fel uppstod: {ex.Message}";
                return RedirectToAction("Profile");
            }
        }
    }
}

