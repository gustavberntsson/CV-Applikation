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
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Bekräftelse av lösenord är nödvändigt.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "De nya lösenorden matchar inte.")]
        public string ConfirmPassword { get; set; }
    }
}