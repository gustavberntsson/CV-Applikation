using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CV_Applikation.Validation
{
    public class UpperCaseValidation : ValidationAttribute
    {
        //Sätter ett standardfelmeddelande.
        public UpperCaseValidation() : base("Lösenordet måste innehålla minst en stor bokstav.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var input = value as string;


            if (string.IsNullOrEmpty(input))
            {
                return ValidationResult.Success;
            }


            //Kollar ifall det finns minst en stor bokstav i strängen.
            if (!Regex.IsMatch(input, @"[A-Z]"))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}