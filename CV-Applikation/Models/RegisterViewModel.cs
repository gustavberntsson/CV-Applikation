using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CV_Applikation.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Du måste ange ett användarnamn")]
        [Display(Name = "Användarnamn")]
        public string AnvandarNamn { get; set; }

        [Required(ErrorMessage = "Du måste ange en epostadress")]
        [Display(Name = "E-post")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Du måste ange ett förnamn")]
        [Display(Name = "Förnamn")]
        public string FörNamn { get; set; }

        [Required(ErrorMessage = "Du måste ange ett efternamn")]
        [Display(Name = "Efternamn")]
        public string EfterNamn { get; set; }

        [Required(ErrorMessage = "Du måste ange en adress")]
        [Display(Name = "Adress")]
        public string Adress { get; set; }

        [Required(ErrorMessage = "Du måste ange ett telefonnummer")]
        [Display(Name = "Telefon")]
        public string TelefonNummer { get; set; }

        [Required(ErrorMessage = "Du måste ange ett lösenord")]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        public string Losenord { get; set; }

        [Required(ErrorMessage = "Du måste bekräfta lösenordet")]
        [Compare("Losenord", ErrorMessage = "Lösenorden matchar inte")]
        [DataType(DataType.Password)]
        [Display(Name = "Bekräfta lösenord")]
        public string BekraftaLosenord { get; set; }

        [Display(Name = "Profilbild (URL)")]
        [Url(ErrorMessage = "Ange en giltig URL")]
        public string? ImageUrl { get; set; }
    }
}
