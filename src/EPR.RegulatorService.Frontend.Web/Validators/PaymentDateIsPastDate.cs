using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

namespace EPR.RegulatorService.Frontend.Web.Validators;

public class PaymentDateIsPastDateAttribute : ValidationAttribute
{    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not PaymentDateViewModel { Day: not null, Month: not null, Year: not null } model)
        {
            return ValidationResult.Success;
        }

        return !IsPastDate(model) ? new ValidationResult(ErrorMessage) : ValidationResult.Success;
    }

    private static bool IsPastDate(PaymentDateViewModel model)
    {
        bool isValidDate = DateOnly.TryParse($"{model.Year.Value:D4}-{model.Month.Value:D2}-{model.Day.Value:D2}",
            out var parsedDate);

        if (!isValidDate)
        {
            return false;
        }
        return parsedDate <= DateOnly.FromDateTime(DateTime.Now);
    }
}