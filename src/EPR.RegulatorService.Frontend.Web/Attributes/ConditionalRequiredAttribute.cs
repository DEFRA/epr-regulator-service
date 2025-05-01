namespace EPR.RegulatorService.Frontend.Web.Attributes;

using System.ComponentModel.DataAnnotations;

public class ConditionalRequiredAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;

    public ConditionalRequiredAttribute(string dependentProperty)
    {
        _dependentProperty = dependentProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_dependentProperty);

        if (property == null)
        {
            return new ValidationResult($"Unknown Property: {_dependentProperty}");
        }

        var dependentValue = property.GetValue(validationContext.ObjectInstance);

        if (dependentValue is bool shouldValidate && shouldValidate)
        {
            if (value == null || (value is decimal decimalValue && decimalValue == 0))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
