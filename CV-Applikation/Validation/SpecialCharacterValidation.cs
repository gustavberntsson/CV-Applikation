using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CV_Applikation.Validation
{
    public class SpecialCharacterValidation : ValidationAttribute
    {
        public SpecialCharacterValidation() : base("Lösenordet måste innehålla minst ett specialtecken.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var input = value as string;

            // Om värdet är null eller tomt, gör ingen validering
            if (string.IsNullOrEmpty(input))
            {
                return ValidationResult.Success;
            }

            // Kontrollera om strängen innehåller minst ett specialtecken
            if (!Regex.IsMatch(input, @"[!@#$%^&*(),.?""':;|<>_+={}\[\]\\/`~&]"))
            {
                return new ValidationResult(ErrorMessage ?? "Lösenordet måste innehålla minst ett specialtecken.");
            }

            return ValidationResult.Success;
        }
    }
}