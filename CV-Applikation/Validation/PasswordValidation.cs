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
        var model = validationContext.ObjectInstance;
        
        // Hämtar den aktuella lösenordsegenskapen dynamiskt
        var currentPasswordProperty = model.GetType().GetProperty(_currentPasswordProperty);
        var currentPassword = currentPasswordProperty?.GetValue(model, null) as string;

        // Om currentPassword finns och är lika med det nya lösenordet, skicka ett fel
        if (!string.IsNullOrEmpty(currentPassword) && currentPassword.Equals(value?.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult("Det nya lösenordet får inte vara samma som det nuvarande lösenordet.");
        }

        return ValidationResult.Success;
    }
}
}
