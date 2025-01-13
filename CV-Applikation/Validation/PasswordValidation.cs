using System.ComponentModel.DataAnnotations;
using System;

namespace CV_Applikation.Validation
{
    public class PasswordValidation : ValidationAttribute
    {
        private readonly string _currentPasswordProperty;

        public PasswordValidation(string currentPasswordProperty)
        {
            _currentPasswordProperty = currentPasswordProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Objektet som valideras hämtas.
            var model = validationContext.ObjectInstance;

            try
            {
                // Den aktuella lösenordsegenskapen hämtas dynamiskt.
                var currentPasswordProperty = model.GetType().GetProperty(_currentPasswordProperty);

                // Kollar om egenskapen finns och hämtar värdet för det nuvarande lösenordet från modellen.
                var currentPassword = currentPasswordProperty?.GetValue(model, null) as string;

                // Ifall det nuvarande lösenordet existerar och är samma som det nya lösenordet skicka att det är fel.
                if (!string.IsNullOrEmpty(currentPassword) && currentPassword.Equals(value?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult("Lösenordet kan inte vara det samma som det nuvarande.");
                }
            }
            catch (Exception ex)
            {
                // Fångar upp eventuella undantag och returnerar ett felmeddelande.
                return new ValidationResult($"Ett fel inträffade vid validering: {ex.Message}");
            }

            return ValidationResult.Success;
        }
    }
}
