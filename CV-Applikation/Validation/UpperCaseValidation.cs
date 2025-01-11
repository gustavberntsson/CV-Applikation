using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CV_Applikation.Validation
{
    public class UpperCaseValidation : ValidationAttribute
    {
        public UpperCaseValidation() : base("Lösenordet måste innehålla minst en stor bokstav.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var input = value as string;

            // Om värdet är null eller tomt, gör ingen validering
            if (string.IsNullOrEmpty(input))
            {
                return ValidationResult.Success;
            }

            // Kontrollera om strängen innehåller minst en stor bokstav
            if (!Regex.IsMatch(input, @"[A-Z]"))
            {
                return new ValidationResult(ErrorMessage ?? "Lösenordet måste innehålla minst en stor bokstav.");
            }

            return ValidationResult.Success;
        }
    }
} 