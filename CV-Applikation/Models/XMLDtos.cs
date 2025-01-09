// Lägg dessa klasser i en ny fil t.ex. XmlDtos.cs i Models-mappen

using System.Xml.Serialization;

namespace CV_Applikation.Models
{
    [Serializable]
    [XmlRoot("UserData")]
    public class UserDataXml
    {
        [XmlElement("Profile")]
        public UserProfileXml Profile { get; set; }

        [XmlArray("CVs")]
        [XmlArrayItem("CV")]
        public List<CVXml> CVs { get; set; }

        [XmlArray("Projects")]
        [XmlArrayItem("Project")]
        public List<ProjectXml> Projects { get; set; }
    }

    [Serializable]
    public class UserProfileXml
    {
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrivate { get; set; }
        public ContactInformationXml ContactInformation { get; set; }
    }

    [Serializable]
    public class ContactInformationXml
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
    }

    [Serializable]
    public class CVXml
    {
        public string CVName { get; set; }
        public string ImagePath { get; set; }

        [XmlArray("Educations")]
        [XmlArrayItem("Education")]
        public List<EducationXml> Educations { get; set; }

        [XmlArray("Languages")]
        [XmlArrayItem("Language")]
        public List<LanguageXml> Languages { get; set; }

        [XmlArray("Skills")]
        [XmlArrayItem("Skill")]
        public List<SkillXml> Skills { get; set; }

        [XmlArray("WorkExperiences")]
        [XmlArrayItem("WorkExperience")]
        public List<WorkExperienceXml> WorkExperiences { get; set; }
    }

    [Serializable]
    public class EducationXml
    {
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public string School { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    [Serializable]
    public class LanguageXml
    {
        public string LanguageName { get; set; }
        public int Level { get; set; }
    }

    [Serializable]
    public class SkillXml
    {
        public string SkillName { get; set; }
    }

    [Serializable]
    public class WorkExperienceXml
    {
        public string Position { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    [Serializable]
    public class ProjectXml
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OwnerId { get; set; }

        [XmlArray("ProjectUsers")]
        [XmlArrayItem("ProjectUser")]
        public List<ProjectUserXml> ProjectUsers { get; set; }
    }

    [Serializable]
    public class ProjectUserXml
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}