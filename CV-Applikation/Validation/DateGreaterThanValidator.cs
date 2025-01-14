using System.ComponentModel.DataAnnotations;


namespace CV_Applikation.Validation
{
    //Anpassade regler skapas för att validera egenskaper i modeller. 
    public class DateGreaterThanValidator : ValidationAttribute 
    {
        //Håller namnet för egenskapen som ska jämföras med.
        private readonly string comparison_property;


        //Tar namnet på egenskapen tex StartDate som jämförelsen görs mot.
        public DateGreaterThanValidator(string comparisonProperty)
        {
            comparison_property = comparisonProperty;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validation_context)
        {

            //Aktuella värdet ("EndDate" värdet) tolkas som sträng.
            var currentValue = value as string;


            //Hämtar dynamiskt egenskapen från modellen som ska jämföras ("StartDate" tex).
            var comparisonProperty = validation_context.ObjectType.GetProperty(comparison_property);
            if (comparisonProperty == null)
            {
                return new ValidationResult($"{comparison_property} hittades inte.");
            }


            //Värdet för jämförande egenskapen ("StartDate" tex) hämtas.
            var comparisonValue = comparisonProperty.GetValue(validation_context.ObjectInstance) as string;

            //Kollar ifall StartDate och EndDate är giltiga datum.
            if (DateTime.TryParse(comparisonValue, out var start_date) &&
                DateTime.TryParse(currentValue, out var end_date))
            {

                //Ifall EndDate är mindre än eller lika med StartDate, felmeddelande.
                if (end_date <= start_date)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }


}

