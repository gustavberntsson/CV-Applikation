using CV_Applikation.Models;

public class SearchResult
{
    public string ProfileName { get; set; }
    public string ImageUrl { get; set; }
    public bool IsPrivate { get; set; }
    public string UserId { get; set; }
    public List<CV> Cvs { get; set; } = new List<CV>();
    // Helper properties för att enkelt komma åt all skills och utbildning
    public List<Skills> AllSkills => Cvs.SelectMany(cv => cv.Skills).DistinctBy(s => s.SkillName).ToList();
    public List<Education> AllEducations => Cvs.SelectMany(cv => cv.Educations).DistinctBy(e => e.School).ToList();
    public List<WorkExperience> AllWorkExperiences => Cvs.SelectMany(cv => cv.WorkExperiences).DistinctBy(w => w.CompanyName).ToList();
    public bool IsEnabled { get; set; }

}