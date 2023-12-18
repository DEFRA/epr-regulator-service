using System.ComponentModel.DataAnnotations;
using EPR.RegulatorService.Frontend.Core.Validations;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Attributes;

public class TelephoneNumberValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var phoneNumber = value?.ToString() ?? string.Empty;

        return TelephoneNumberValidator.IsValid(phoneNumber) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }
}