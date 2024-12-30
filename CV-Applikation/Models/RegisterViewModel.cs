using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CV_Applikation.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Du måste ange ett användarnamn")]
        [Display(Name = "Användarnamn")]
        public string AnvandarNamn { get; set; }

        [Required(ErrorMessage = "Du måste ange ett lösenord")]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        public string Losenord { get; set; }

        [Required(ErrorMessage = "Du måste bekräfta lösenordet")]
        [Compare("Losenord", ErrorMessage = "Lösenorden matchar inte")]
        [DataType(DataType.Password)]
        [Display(Name = "Bekräfta lösenord")]
        public string BekraftaLosenord { get; set; }

        [Display(Name = "Profilbild URL")]
        [Url(ErrorMessage = "Ange en giltig URL")]
        public string? ImageUrl { get; set; }
    }
}
