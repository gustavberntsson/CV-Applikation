using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CV_Applikation.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vänligen skriv ett användarnamn.")]
        [StringLength(255, ErrorMessage = "Användarnamnet får inte överskrida 255 tecken.")]
        public string AnvandarNamn { get; set; }
        [Required(ErrorMessage = "Vänligen skriv lösenord.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenordet måste vara mellan 6 och 100 tecken.")]
        public string Losenord { get; set; }
        public bool RememberMe { get; set; }
    }
}
