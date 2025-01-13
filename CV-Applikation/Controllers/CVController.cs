using CV_Applikation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CV_Applikation.Controllers
{
    public class CVController : Controller
    {
        private UserContext context;
        private readonly IWebHostEnvironment host;
        public CVController(UserContext service, IWebHostEnvironment webHostEnvironment) 
        {
            context = service;
            host = webHostEnvironment;
        }

        [Authorize]
        //Enbart inloggade användare som autentiserats har åtkomst till metoden.
        [HttpGet]
        public IActionResult Add()
        {
            //En ny instans av modellen CV skapas.
            //Modellen är tom, för att möjliggöra att användaren ska kunna fylla i ett nytt CV.
            CV cv = new CV();
            //En vy(webbsida) returneras till klienten (webbläsaren) med modellen cv.
            //Modellen cv används i vyn för att genom ett formulär kunna skapa ett CV.
            return View(cv);
        }


        [HttpPost]
        public async Task<IActionResult> Add(CV cv, List<Education> educations, List<Skills> skills, List<WorkExperience> workExp, List<Languages> languages, IFormFile? ImagePath)
        {
            //Hämtar användarens id från inloggad användare.
            var id_user = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            cv.UserId = id_user;

            if (string.IsNullOrEmpty(id_user))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Om man väljer en image path
                    if (ImagePath != null && ImagePath.Length > 0)
                    {

                        // Lägger in filen i wwwroot/images
                        var file_extension = Path.GetExtension(ImagePath.FileName);
                        string fileName_unique = Guid.NewGuid().ToString() + file_extension;
                        string folder_upload = Path.Combine(host.WebRootPath, "images");
                        string file_path = Path.Combine(folder_upload, fileName_unique);

                        using (var stream = new FileStream(file_path, FileMode.Create, FileAccess.Write))
                        {
                            await ImagePath.CopyToAsync(stream);
                        }

                        cv.ImagePath = "/images/" + fileName_unique;
                    }

                    // Lägg till CV
                    context.Add(cv);
                    await context.SaveChangesAsync();

                    // Lägg till utbildningar, färdigheter, arbetslivserfarenhet, och språk
                    if (educations != null && educations.Any())
                    {
                        foreach (var education in educations)
                        {
                            var existingEducation = context.Educations
                                .FirstOrDefault(e => e.CVId == cv.CVId &&
                                                     e.School == education.School &&
                                                     e.Degree == education.Degree &&
                                                     e.FieldOfStudy == education.FieldOfStudy &&
                                                     e.StartDate == education.StartDate &&
                                                     e.EndDate == education.EndDate);

                            if (existingEducation == null)
                            {
                                education.CVId = cv.CVId;
                                context.Educations.Add(education);
                            }
                        }
                    }

                    if (skills != null && skills.Any())
                    {
                        foreach (var skill in skills)
                        {
                            var existingSkill = context.Skills
                                .FirstOrDefault(s => s.CVId == cv.CVId &&
                                                     s.SkillName == skill.SkillName);

                            if (existingSkill == null)
                            {
                                skill.CVId = cv.CVId;
                                context.Skills.Add(skill);
                            }
                        }
                    }

                    if (workExp != null && workExp.Any())
                    {
                        foreach (var workExperience in workExp)
                        {
                            var existingWorkExp = context.WorkExperiences
                                .FirstOrDefault(w => w.CVId == cv.CVId &&
                                                     w.CompanyName == workExperience.CompanyName &&
                                                     w.Description == workExperience.Description &&
                                                     w.Position == workExperience.Position &&
                                                     w.StartDate == workExperience.StartDate &&
                                                     w.EndDate == workExperience.EndDate);

                            if (existingWorkExp == null)
                            {
                                workExperience.CVId = cv.CVId;
                                context.WorkExperiences.Add(workExperience);
                            }
                        }
                    }

                    if (languages != null && languages.Any())
                    {
                        foreach (var language in languages)
                        {
                            var existingLanguage = context.Languages
                                .FirstOrDefault(l => l.CVId == cv.CVId &&
                                                     l.LanguageName == language.LanguageName &&
                                                     l.Level == language.Level);

                            if (existingLanguage == null)
                            {
                                language.CVId = cv.CVId;
                                context.Languages.Add(language);
                            }
                        }
                    }

                    context.Update(cv);
                    await context.SaveChangesAsync();

                    return RedirectToAction("Index", "home");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ett fel har inträffat: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Något gick fel vid hämtning av CV.");
                    return View(cv);
                }
            }
            else
            {
                return View(cv);
            }
        }

        [HttpGet]
        public IActionResult EditCv(int cvId)
        {

            //"Include" används för att ladda relaterade data som utbildningar, språk, färdigheter etc.
            //CV:t och alla relaterade tabeller hämtas.
            var cv = context.CVs
                .Include(c => c.Educations)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Languages)
                .Include(c => c.Skills)
                .FirstOrDefault(c => c.CVId == cvId); //Första CV:t hämtas som matchar CVId 



            var viewModel = new EditCvViewModel
            {
                CVId = cv.CVId,
                CVName = cv.CVName,
                Educations = cv.Educations ?? new List<Education>(),
                WorkExperiences = cv.WorkExperiences ?? new List<WorkExperience>(), //Vid null skapa en tom lista.
                Languages = cv.Languages ?? new List<Languages>(),
                Skills = cv.Skills ?? new List<Skills>(),
                IsPrivate = cv.IsPrivate

            };
            //Vyn returneras med viewModel-objektet som skapats.
            //Vyn får åtkomst till data som behövs för att redigera CV:t.
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> EditCv(EditCvViewModel model)
        {
            // Validera modellen
            if (ModelState.IsValid)
            {
                try
                {
                    // Hämta CV:t och relaterade data
                    var cv = context.CVs
                        .Include(c => c.Educations)
                        .Include(c => c.WorkExperiences)
                        .Include(c => c.Languages)
                        .Include(c => c.Skills)
                        .FirstOrDefault(c => c.CVId == model.CVId);

                    if (cv == null)
                    {
                        return NotFound(); // Returnera 404 om CV inte finns
                    }

                    // Uppdatera CV:t
                    cv.CVName = model.CVName;
                    cv.IsPrivate = model.IsPrivate;

                    // Lägg till nya utbildningar, färdigheter, arbetslivserfarenhet och språk
                    var new_education = new List<Education>();
                    foreach (var education in model.Educations)
                    {
                        var existingEducation = cv.Educations.FirstOrDefault(e => e.Id == education.Id);
                        if (existingEducation != null)
                        {
                            existingEducation.School = education.School;
                            existingEducation.Degree = education.Degree;
                            existingEducation.FieldOfStudy = education.FieldOfStudy;
                            existingEducation.StartDate = education.StartDate;
                            existingEducation.EndDate = education.EndDate;
                        }
                        else
                        {
                            new_education.Add(new Education
                            {
                                School = education.School,
                                Degree = education.Degree,
                                FieldOfStudy = education.FieldOfStudy,
                                StartDate = education.StartDate,
                                EndDate = education.EndDate,
                            });
                        }
                    }

                    cv.Educations.AddRange(new_education);

                    var new_skills = new List<Skills>();
                    foreach (var skill in model.Skills)
                    {
                        var existingSkill = cv.Skills.FirstOrDefault(s => s.Id == skill.Id);
                        if (existingSkill != null)
                        {
                            existingSkill.SkillName = skill.SkillName;
                        }
                        else
                        {
                            new_skills.Add(new Skills
                            {
                                SkillName = skill.SkillName
                            });
                        }
                    }
                    cv.Skills.AddRange(new_skills);

                    var new_workExperience = new List<WorkExperience>();
                    foreach (var workExperience in model.WorkExperiences)
                    {
                        var existingWorkExperience = cv.WorkExperiences.FirstOrDefault(w => w.Id == workExperience.Id);
                        if (existingWorkExperience != null)
                        {
                            existingWorkExperience.CompanyName = workExperience.CompanyName;
                            existingWorkExperience.Position = workExperience.Position;
                            existingWorkExperience.Description = workExperience.Description;
                            existingWorkExperience.StartDate = workExperience.StartDate;
                            existingWorkExperience.EndDate = workExperience.EndDate;
                        }
                        else
                        {
                            new_workExperience.Add(new WorkExperience
                            {
                                CompanyName = workExperience.CompanyName,
                                Position = workExperience.Position,
                                Description = workExperience.Description,
                                StartDate = workExperience.StartDate,
                                EndDate = workExperience.EndDate
                            });
                        }
                    }
                    cv.WorkExperiences.AddRange(new_workExperience);

                    var new_languages = new List<Languages>();
                    foreach (var language in model.Languages)
                    {
                        var existingLanguage = cv.Languages.FirstOrDefault(l => l.Id == language.Id);
                        if (existingLanguage != null)
                        {
                            existingLanguage.LanguageName = language.LanguageName;
                            existingLanguage.Level = language.Level;
                        }
                        else
                        {
                            new_languages.Add(new Languages
                            {
                                LanguageName = language.LanguageName,
                                Level = language.Level
                            });
                        }
                    }
                    cv.Languages.AddRange(new_languages);

                    // Ladda upp ny bild
                    if (model.ImagePath != null && model.ImagePath.Length > 0)
                    {
                        // Lägg till filen i wwwroot/images
                        string fileName_unique = Guid.NewGuid().ToString() + Path.GetExtension(model.ImagePath.FileName);
                        string folder_upload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        string filePath = Path.Combine(folder_upload, fileName_unique);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImagePath.CopyToAsync(stream);
                        }

                        cv.ImagePath = "/images/" + fileName_unique;
                    }

                    context.Update(cv);
                    await context.SaveChangesAsync();

                    return RedirectToAction("Profile", "Account");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ett fel har inträffat: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Något gick fel vid hämtning av CV.");
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

    }
}



