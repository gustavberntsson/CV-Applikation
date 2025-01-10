using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Validation
{
    public class EpostValidation : ValidationAttribute
    {
        public EpostValidation() : base("Ogiltig e-postadress.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var email = value as string;

            // Om e-post är null eller tom, gör ingen validering
            if (string.IsNullOrEmpty(email))
            {
                return ValidationResult.Success;
            }

            // Använder vår egen valideringsmetod
            if (!Validation.CheckEmail(email))
            {
                return new ValidationResult(ErrorMessage ?? "Ogiltig e-postadress.");
            }

            return ValidationResult.Success;
        }
    }
}
