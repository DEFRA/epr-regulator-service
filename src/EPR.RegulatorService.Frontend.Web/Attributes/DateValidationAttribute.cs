using System.ComponentModel.DataAnnotations;
using System.Globalization;


using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateValidationAttribute(string cancellationDateErrorMessage,string dateErrorMessage) : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var viewModel = (CancelDateRegistrationSubmissionViewModel)validationContext.ObjectInstance;

            int? day = viewModel.Day;
            int? month = viewModel.Month;
            int? year = viewModel.Year;

            string dateString = $"{day}/{month}/{year}";
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy" };

            bool isDateValid = DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var validDate);

            if (!isDateValid)
            {
                return new ValidationResult(cancellationDateErrorMessage);
            }

            if(validDate < DateTime.Now)
            {
                return new ValidationResult(dateErrorMessage);
            }

            viewModel.CancellationDate = validDate;

            return ValidationResult.Success;
        }


    }
}