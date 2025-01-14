using System.ComponentModel.DataAnnotations;
using CV_Applikation.Validation;

namespace CV_Applikation.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Du måste ange ett användarnamn")]
        [Validation.UsernameValidation(ErrorMessage = "Ogiltigt användarnamn. Endast bokstäver (A-Z, a-z) och siffror är tillåtna.")]
        [Display(Name = "Användarnamn")]
        public string AnvandarNamn { get; set; }

        [Required(ErrorMessage = "Du måste ange en epostadress")]
        [Validation.EpostValidation(ErrorMessage = "Ogiltig e-postadress, korrekt format är exempel@exempel.com")]
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
        [Validation.PhoneValidation(ErrorMessage = "Telefonnumret måste vara 10 siffror. Korrekt format är 070-1234567.")]
        [Display(Name = "Telefon")]
        public string TelefonNummer { get; set; }

        [Required(ErrorMessage = "Du måste ange ett lösenord")]
        [StringLength(100, ErrorMessage = "Lösenordet måste vara minst {2} tecken långt.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*\d).*$", ErrorMessage = "Lösenordet måste innehålla minst en siffra.")]
        [SpecialCharacterValidation(ErrorMessage = "Ett specialtecken är obligatoriskt")]
        [UpperCaseValidation(ErrorMessage = "En storbokstav är obligatoriskt")]
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
