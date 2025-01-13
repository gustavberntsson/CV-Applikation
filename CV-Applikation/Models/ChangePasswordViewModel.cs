using CV_Applikation.Validation;
using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class ChangePasswordViewModel
    {


        [Required(ErrorMessage = "Nuvarande lösenord är nödvändigt.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Nytt lösenord är nödvändigt.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Lösenordet måste vara minst {2} tecken långt.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*\d).*$", ErrorMessage = "Lösenordet måste innehålla minst en siffra.")]
        [PasswordValidation("CurrentPassword", ErrorMessage = "Det nya lösenordet får inte vara samma som det nuvarande lösenordet.")]
        [SpecialCharacterValidation(ErrorMessage ="Ett specialtecken är obligatoriskt")]
        [UpperCaseValidation(ErrorMessage = "En storbokstav är obligatoriskt")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Bekräftelse av lösenord är nödvändigt.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "De nya lösenorden matchar inte.")]
        public string ConfirmPassword { get; set; }
    }
}