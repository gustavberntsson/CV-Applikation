using System.ComponentModel.DataAnnotations;

namespace CV_Applikation.Models
{
    public class DateGreaterThanValidator : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        public DateGreaterThanValidator(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = value as string;

            // Hämta värdet för StartDate
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (comparisonProperty == null)
            {
                return new ValidationResult($"Property {_comparisonProperty} not found.");
            }

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance) as string;

            // Kontrollera att både StartDate och EndDate är giltiga datum
            if (DateTime.TryParse(comparisonValue, out var startDate) &&
                DateTime.TryParse(currentValue, out var endDate))
            {
                if (endDate <= startDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} måste vara efter startdatumet.");
                }
            }

            return ValidationResult.Success;
        }
    }


}
