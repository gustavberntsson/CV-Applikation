using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Validation
{
    public class PhoneValidation : ValidationAttribute
    {
        public PhoneValidation() : base("Ogiltigt telefonnummer.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var phone = value as string;

            // Om telefonnummer är null eller tomt, gör ingen validering
            if (string.IsNullOrEmpty(phone))
            {
                return ValidationResult.Success;
            }

            // Anropar vår egen valideringsmetod
            if (!Validation.CheckPhone(phone))
            {
                return new ValidationResult(ErrorMessage ?? "Ogiltigt telefonnummer.");
            }

            return ValidationResult.Success;
        }
}
}
