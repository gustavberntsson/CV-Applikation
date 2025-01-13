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
            //Inloggade användarens ID hämtas från claims (autentiseringstoknen),
            var id_user = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            //Användarens ID (id_user) tilldelas till CV-objektet.
            //Kopplar CV:t till inloggade användaren.
            cv.UserId = id_user;
            //Ifall användaren inte är inloggad skickas användaren till inloggninssidan.
            if (string.IsNullOrEmpty(id_user))
            {

                return RedirectToAction("Login", "Account");
            }

            //Kontroll gällande ifall data är giltig enligt modellen (ModelState) som användaren angav i forumläret.
            //Ogiltig modell returnerar fel, och användaren blir kvar på samma sida.
            if (ModelState.IsValid)
            {

                //Kontrollerar ifall uppladdningen av en bildfil skett, ifall ImagePath har innehåll och inte är null.
                if (ImagePath != null && ImagePath.Length > 0)
                {

                    //Filändelsen för den uppladade filen hämtas (tex "png").
                    var file_extension = Path.GetExtension(ImagePath.FileName);
                    //Ett unikt filnamn genereras med en GUID (globalt unikt ID).
                    string fileName_unique = Guid.NewGuid().ToString() + file_extension;
                    //Sökvägen till mappen där bilder för CV sparas.
                    string folder_upload = Path.Combine(host.WebRootPath, "images");
                    //Uppladdningsmappens sökväg kombineras med det unika filnamnet.
                    //Skapande av fullständig sökväg som filen sparas vid.
                    string file_path = Path.Combine(folder_upload, fileName_unique);
                    //Filström öppnas för sparande av den uppladade filen.
                    using (var stream = new FileStream(file_path, FileMode.Create, FileAccess.Write))
                    {

                        //Innehållet från den uppladdade filen (ImagePath) kopieras till filströmmen.
                        await ImagePath.CopyToAsync(stream);
                    }


                    //I CV-objektets ImagePath sätts sökvägen till den sparade filen.
                    //Sökvägen kan användas för att sen referera till bilden, tex visa den i gränssnittet.
                    cv.ImagePath = "/images/" + fileName_unique;
                }


                //Nya CV:t läggs till i databasen.
                context.Add(cv);
                //Alla ändringar sparas asynkront till databasen.
                await context.SaveChangesAsync();


                //Kontroll - ifall det existerar utbildningar att lägga till för CV:t.
                if (educations != null && educations.Any())
                {
                    foreach (var education in educations)
                    {

                        //Kontroll - ifall en liknande utbildning existerar i databasen redan för det nuvarande CV:t.
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
                            //Läggs till i databasen
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
            else
            {

                //Valideringsfelen loggas till konsolen (felsökning).
                //ModelState - modellens valideringsstatus
                foreach (var key in ModelState.Keys)
                {
                    //Valideringsfel hämtas för aktuell nyckel
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        //Fältets namn (key) loggas, samt felmeddelandet. 
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                //Samma vy returneras med det aktuella CV-objektet.
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

            //Kontroll - ifall modellen följer valideringsreglerna.
            if (ModelState.IsValid)
            {

                //CV:t hämtas från databasen utifrån modellens (model) CVId.
                var cv = context.CVs
                .Include(c => c.Educations)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Languages)
                .Include(c => c.Skills)
                .FirstOrDefault(c => c.CVId == model.CVId);

                //Kontrollerar om CV:t i databasen inte hittas.
                if (cv == null)
                {
                    return NotFound(); //En 404-sida (NotFound) returneras.
                }


                //Namn på CV:t uppdateras utifrån datan som användaren matat in.
                cv.CVName = model.CVName;

                cv.IsPrivate = model.IsPrivate;

                //En ny lista skapas för nya utbildningar som CV:t ska inkludera. 
                var new_education = new List<Education>();

                foreach (var education in model.Educations)
                {

                    //Kollar ifall utbildningen redan finns i det aktuella CV:t utifrån dess Id.
                    var existingEducation = cv.Educations
                        .FirstOrDefault(e => e.Id == education.Id);
                    //Ifall utbildningen redan existerar uppdateras den, redigeras.
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

                //Nya utbildningar som läggs till i CV:t.
                cv.Educations.AddRange(new_education);

                //Uppdatera färdigheter
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

                //Uppdatera tidigare erfarenheter

                var new_workExperience = new List<WorkExperience>();

                foreach (var workExperience in model.WorkExperiences)
                {
                    var existingWorkExperience = cv.WorkExperiences
                        .FirstOrDefault(w => w.Id == workExperience.Id);

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


                //Uppdatera språk
                var new_languages = new List<Languages>();
                foreach (var language in model.Languages)
                {
                    var existingLanguage = cv.Languages
                        .FirstOrDefault(l => l.Id == language.Id);

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

                //Ladda upp ny bild
                if (model.ImagePath != null && model.ImagePath.Length > 0)
                {
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
            else
            {
                return View(model);
            }
        }

    }
}



