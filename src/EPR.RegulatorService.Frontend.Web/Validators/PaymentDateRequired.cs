using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

namespace EPR.RegulatorService.Frontend.Web.Validators;

public class PaymentDateRequiredAttribute : ValidationAttribute
{    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not PaymentDateViewModel model ||
            model.Day == null ||
            model.Month == null ||
            model.Year == null)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}