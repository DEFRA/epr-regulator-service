using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateValidationAttribute(
        string dayErrorMessage,
        string monthErrorMessage,
        string yearErrorMessage,
        string dateErrorMessage) : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string? text = value.ToString();
                if (text != null && text.Length > 10)
                {
                    return new ValidationResult(dateErrorMessage);
                }
                return ValidationResult.Success;
            }
            return new ValidationResult(monthErrorMessage);
        }
    }
}