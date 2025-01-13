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


            //Ifall värdet är tomt eller null, ingen validering.
            if (string.IsNullOrEmpty(input))
            {
                return ValidationResult.Success;
            }


            //Kollar ifall minst ett specialtecken förekommer i strängen.
            if (!Regex.IsMatch(input, @"[!@#$%^&*(),.?""':;|<>_+={}\[\]\\/`~&]"))
            {
                return new ValidationResult(ErrorMessage ?? "Lösenordet måste innehålla minst ett specialtecken.");
            }

            return ValidationResult.Success;
        }
    }
}