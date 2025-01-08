using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class StartDateValidator : ValidationAttribute
    {
    
        public StartDateValidator()
        {
            ErrorMessage = "Startdatum kan inte vara i framtiden.";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true; // Om det är null, validiteten kontrolleras av [Required] attributet.
            }

            DateTime parsedDate;

            // Försök att parsning värdet till DateTime
            if (DateTime.TryParse(value.ToString(), out parsedDate))
            {
                // Kontrollera om datumet är i framtiden
                return parsedDate <= DateTime.Now;
            }

            return false; // Om inte ett giltigt datum, är det ogiltigt.
        }
    }
}

