namespace EPR.RegulatorService.Frontend.Web.Attributes;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property)]

public partial class CurrencyValidationAttribute : ValidationAttribute
{
    private const int MAX_CHARACTER_COUNT = 17;
    private readonly string _requiredErrorMessage;
    private readonly string _valueExceededMessage;
    private readonly string _invalidFormatMessage;
    private readonly decimal _maxValue;

    public CurrencyValidationAttribute( string requiredErrorMessage,
                                        string valueExceededMessage,
                                        string invalidFormatMessage,
                                        string maxValue)
    {
        _requiredErrorMessage = requiredErrorMessage;
        _valueExceededMessage = valueExceededMessage;
        _invalidFormatMessage = invalidFormatMessage;
        if (!decimal.TryParse(maxValue, out _maxValue))
        {
            throw new InvalidDataException("MaxValue is incorrect");
        }
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string? text = value.ToString();
            if (text != null && text.Length > MAX_CHARACTER_COUNT)
            {
                return new ValidationResult(_valueExceededMessage);
            }

            if (text != null && IsValidCurrency(text, out decimal theValue))
            {
                if (theValue > _maxValue)
                {
                    return new ValidationResult(_valueExceededMessage);
                }
            } else
            {
                return new ValidationResult(_invalidFormatMessage);
            }

            return ValidationResult.Success;
        }

        return new ValidationResult(_requiredErrorMessage);
    }

    private static bool IsValidCurrency(string input, out decimal theValue)
    {
        theValue = 0M;
        var regex = TestRegex();

        if (!regex.IsMatch(input))
        {
            return false;
        }

        string cleanedInput = input.Replace(",", "");

        return decimal.TryParse(cleanedInput, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out theValue);
    }

    [GeneratedRegex(@"^-?(\d{1,3})(,\d{3})*(\.\d{1,2})?$")]
    private static partial Regex TestRegex();
}
