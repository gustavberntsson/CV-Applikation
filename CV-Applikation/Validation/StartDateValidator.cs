using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Validation
{
    public class StartDateValidator : ValidationAttribute
    {

        public StartDateValidator()
        {
            ErrorMessage = "Startdatum är obligatoriskt";
        }

        public override bool IsValid(object value)
        {

            //Kollar ifall värdet är null.
            if (value == null)
            {
                return true; //Ifall värdet är null tillåts det.
            }

            DateTime parsedDate;

            //Försök med att omvandla värdet till DateTime-objekt.
            if (DateTime.TryParse(value.ToString(), out parsedDate))
            {

                //Kollar ifall datumet finns i framtiden.
                return parsedDate <= DateTime.Now;
            }

            return false; // Ifall värdet inte parsas till ett giltigt datum, ogiltigt datum då.
        }
    }
}
