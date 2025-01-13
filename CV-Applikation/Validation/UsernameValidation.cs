using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CV_Applikation.Validation
{
    public class UsernameValidation : ValidationAttribute
    {
        public UsernameValidation() : base("Ogiltigt användarnamn. Endast bokstäver (A-Z, a-z) och siffror är tillåtna.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var username = value as string;

            // Om användarnamnet är null eller tomt, gör ingen validering
            if (string.IsNullOrEmpty(username))
            {
                return ValidationResult.Success;
            }

            // Kontrollera om användarnamnet endast innehåller bokstäver och siffror
            if (!Regex.IsMatch(username, "^[a-zA-Z0-9]+$"))
            {
                return new ValidationResult(ErrorMessage ?? "Ogiltigt användarnamn. Endast bokstäver (A-Z, a-z) och siffror är tillåtna.");
            }

            return ValidationResult.Success;
        }
    }
}
