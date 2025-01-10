using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class EditProfileViewModel
    {

        [Required(ErrorMessage = "Du måste ange en giltig e-postadress")]
        [Validation.EpostValidation(ErrorMessage = "Ogiltig e-postadress, korrekt format är exempel@exempel.com")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Du måste ange ett förnamn.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Du måste ange ett efternamn.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Du måste ange en adress.")]
        public string Adress { get; set; }
        [Required(ErrorMessage = "Du måste ange ett telefonnummer.")]
        [Validation.PhoneValidation(ErrorMessage = "Ogiltigt telefonnummer, korrekt format är 070-1234567")]
        public string PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }
        public bool IsPrivate { get; set; }

    }
}