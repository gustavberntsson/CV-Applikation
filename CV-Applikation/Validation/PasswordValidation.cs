using System;
using System.ComponentModel.DataAnnotations;
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

            //Objektet som valideras hämtas.
            var model = validationContext.ObjectInstance;

            //Den aktuella lösenordsegenskapen hämtas dynamiskt.
            var currentPasswordProperty = model.GetType().GetProperty(_currentPasswordProperty);


            //Värdet för det nuvarande lösenordet från modellen hämtas.
            var currentPassword = currentPasswordProperty?.GetValue(model, null) as string;


            //Ifall det nuvarande lösenordet existerar och är samma som det nya lösenordet skicka att det är fel.
            if (!string.IsNullOrEmpty(currentPassword) && currentPassword.Equals(value?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ValidationResult("");
            }

            return ValidationResult.Success;
        }
   }
}
