using CV_Applikation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Controllers
{
    public class CVController : Controller
    {
        private UserContext context;
        public CVController(UserContext service) 
        {
            context = service;
        }
        public IActionResult CvDetails(int id) //c
        {
            var cv = context.CVs
            .Include(c => c.User)
            .Include(c => c.Educations)
            .Include(c => c.Skills)
            .Include(c => c.WorkExperiences)
            .FirstOrDefault(c => c.CVId == id);

            if (cv == null) return NotFound();

            return View(cv);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            CV cv = new CV();
            return View(cv);
        }

        [HttpPost]
        public IActionResult Add(CV cv, List<Education> educations, List<Skills> skills, List<WorkExperience> workExp, List<Languages> languages)
        {
            //cc
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

               //cc
                // Koppla CV till inloggad användare
                //cv.OwnerId = userId;
            cv.UserId = userId;  // Sätt UserId också (för att möta krav från databasen)
            context.Add(cv);
            context.SaveChanges();
            educations = educations.DistinctBy(e => new { e.School, e.Degree, e.StartDate }).ToList();
            skills = skills.DistinctBy(s => s.SkillName).ToList();  // Exempel för Skills
           workExp = workExp.DistinctBy(w => new { w.CompanyName, w.StartDate, w.EndDate }).ToList();
            languages = languages.DistinctBy(l => l.LanguageName).ToList();
            educations = educations.DistinctBy(e => new { e.School, e.Degree, e.StartDate }).ToList();
            skills = skills.DistinctBy(s => s.SkillName).ToList();  // Exempel för Skills
            workExp = workExp.DistinctBy(w => new { w.CompanyName, w.StartDate, w.EndDate }).ToList();  // Exempel för WorkExperience
            languages = languages.DistinctBy(l => l.LanguageName).ToList();  // Exempel för Languages

            // Lägg till relaterade objekt (om de inte redan finns)
            if (educations != null && educations.Any())
            {
                foreach (var education in educations)
                {
                    var existingEducation = context.Educations
                        .FirstOrDefault(e => e.CVId == cv.CVId &&
                                             e.School == education.School &&
                                             e.Degree == education.Degree &&
                                             e.StartDate == education.StartDate);

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
                        .FirstOrDefault(s => s.CVId == cv.CVId && s.SkillName == skill.SkillName);

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
                        .FirstOrDefault(l => l.CVId == cv.CVId && l.LanguageName == language.LanguageName);

                    if (existingLanguage == null)
                    {
                        language.CVId = cv.CVId;
                        context.Languages.Add(language);
                    }
                }
            }

            //ej cc
            //if (educations != null && educations.Any())
            //{
            //    foreach (var education in educations)
            //    {
            //        education.CVId = cv.CVId; // Koppla till det nya CV:t
            //        context.Educations.Add(education);
            //        //cv.Educations.Add(education); // Lägg till i CV-objektet
            //    }
            //}

            //if (skills != null && skills.Any())
            //{
            //    foreach (var skill in skills)
            //    {
            //        skill.CVId = cv.CVId; // Koppla till det nya CV:t
            //        context.Skills.Add(skill);
            //        //cv.Skills.Add(skill); // Lägg till i CV-objektet
            //    }
            //}

            //if (workExp != null && workExp.Any())
            //{
            //    foreach (var workExperience in workExp)
            //    {
            //        workExperience.CVId = cv.CVId; // Koppla till det nya CV:t
            //        context.WorkExperiences.Add(workExperience);
            //        //cv.WorkExperiences.Add(workExperience); // Lägg till i CV-objektet
            //    }
            //}

            //if (languages != null && languages.Any())
            //{
            //    foreach (var language in languages)
            //    {
            //        language.CVId = cv.CVId; // Koppla språk till CV
            //        context.Languages.Add(language); // Lägg direkt till i databasen
            //        //cv.Languages.Add(language); // Lägg till i minnesrepresentationen av CV

            //    }
            //}
            //educations = educations.DistinctBy(e => new { e.School, e.Degree, e.StartDate }).ToList();
            //skills = skills.DistinctBy(s => s.SkillName).ToList();  // Exempel för Skills
            //workExp = workExp.DistinctBy(w => new { w.CompanyName, w.StartDate, w.EndDate }).ToList();
            //languages = languages.DistinctBy(l => l.LanguageName).ToList();
            context.Update(cv); // Uppdaterar CV:t och alla relaterade objekt
            context.SaveChanges(); // Sparar ändringar
            return RedirectToAction("Index", "home");
        }
        [HttpGet]
        public IActionResult EditCv(int cvId)
        {
            var cv = context.CVs
                .Include(c => c.Educations)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Languages)
                .Include(c => c.Skills)
                .FirstOrDefault(c => c.CVId == cvId);

            if (cv == null)
            {
                return NotFound(); // Returnerar 404 om CV:t inte hittas
            }

            var viewModel = new EditCvViewModel
            {
                CVId = cv.CVId,
                CVName = cv.CVName,
                Educations = cv.Educations ?? new List<Education>(), // Om null, skapa en tom lista
                WorkExperiences = cv.WorkExperiences ?? new List<WorkExperience>(), // Om null, skapa en tom lista
                Languages = cv.Languages ?? new List<Languages>(), // Om null, skapa en tom lista
                Skills = cv.Skills ?? new List<Skills>() // Om null, skapa en tom lista
            };

            return View(viewModel);
        }


        /* [HttpGet]
         public async Task<IActionResult> EditCv(int cvId)
         {
             var cv = await context.CVs
                 .Include(c => c.Educations)
                 .Include(c => c.WorkExperiences)
                 .Include(c => c.Languages)
                 .Include(c => c.Skills)
                 .FirstOrDefaultAsync(c => c.CVId == cvId);

             if (cv == null)
             {
                 return NotFound();
             }

             var viewModel = new EditCvViewModel
             {
                 CVId = cv.CVId,
                 CVName = cv.CVName,
                 Educations = cv.Educations.ToList(),
                 WorkExperiences = cv.WorkExperiences.ToList(),
                 Languages = cv.Languages.ToList(),
                 Skills = cv.Skills.ToList()
             };

             return View(viewModel);
         }*/

        [HttpPost]
        public IActionResult EditCv(EditCvViewModel model)
        {

            if (ModelState.IsValid)
            {


                var cv = context.CVs
                    .Include(c => c.Educations)
                    .Include(c => c.WorkExperiences)
                    .Include(c => c.Languages)
                    .Include(c => c.Skills)
                    .FirstOrDefault(c => c.CVId == model.CVId);

                if (cv == null)
                {
                    return NotFound();
                }

                cv.CVName = model.CVName;
                cv.Educations = model.Educations;
                cv.WorkExperiences = model.WorkExperiences;
                cv.Languages = model.Languages;
                cv.Skills = model.Skills;

                context.SaveChanges();  // Spara ändringarna i databasen

                return RedirectToAction("Profile", "Account");  // Eller en annan vy som du vill skicka användaren till

            }
            return View(model);  // Återgå till samma vy vid ogiltig data
        }
    }
}
