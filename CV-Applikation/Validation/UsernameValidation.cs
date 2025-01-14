using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CV_Applikation.Models;
using Microsoft.EntityFrameworkCore;

namespace CV_Applikation.Validation
{
    public class UsernameValidation : ValidationAttribute
    {
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
                return new ValidationResult("Ogiltigt användarnamn. Endast bokstäver (A-Z, a-z) och siffror är tillåtna.");
            }

            // Hämta database context från validationContext
            var dbContext = validationContext.GetService(typeof(UserContext)) as UserContext;
            if (dbContext == null)
            {
                throw new InvalidOperationException("UserContext kunde inte hittas.");
            }

            // Kontrollera om användarnamnet redan finns i databasen
            var userExists = dbContext.Users.Any(u => u.UserName == username);
            if (userExists)
            {
                return new ValidationResult("Användarnamnet är upptaget.");
            }

            return ValidationResult.Success;
        }
    }
}