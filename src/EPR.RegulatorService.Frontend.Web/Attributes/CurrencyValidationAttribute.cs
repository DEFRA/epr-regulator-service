namespace EPR.RegulatorService.Frontend.Web.Attributes;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property)]

public partial class CurrencyValidationAttribute : ValidationAttribute
{
    private const int MAX_CHARACTER_COUNT = 17;
    private readonly bool _validateSpecialCharacters;
    private readonly bool _validateNonNumericCharacters;
    private readonly string _requiredErrorMessage;
    private readonly string _valueExceededMessage;
    private readonly string _invalidFormatMessage;
    private readonly string _specialCharactersMessage;
    private readonly string _nonNumericMessage;
    private readonly decimal _maxValue;

    public CurrencyValidationAttribute(string requiredErrorMessage,
                                        string valueExceededMessage,
                                        string invalidFormatMessage,
                                        string maxValue,
                                        string specialCharactersMessage = null,
                                        string nonNumericMessage = null)
    {
        _validateSpecialCharacters = !string.IsNullOrEmpty(specialCharactersMessage);
        _validateNonNumericCharacters = !string.IsNullOrEmpty(nonNumericMessage);
        _requiredErrorMessage = requiredErrorMessage;
        _valueExceededMessage = valueExceededMessage;
        _invalidFormatMessage = invalidFormatMessage;
        _specialCharactersMessage = specialCharactersMessage;
        _nonNumericMessage = nonNumericMessage;

        if (!decimal.TryParse(maxValue, NumberStyles.Currency, CultureInfo.CurrentCulture, out _maxValue))
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue));
        }
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        string? text = value?.ToString();

        if (string.IsNullOrEmpty(text))
        {
            return new ValidationResult(_requiredErrorMessage);
        }

        if (ExceedsMaxCharacterCount(text) || ContainsSpecialCharacters(text) || ContainsNonNumericCharacters(text))
        {
            return new ValidationResult(GetValidationErrorMessage(text));
        }

        if (!IsValidCurrency(text, out decimal theValue))
        {
            return new ValidationResult(_invalidFormatMessage);
        }

        return ExceedsMaxValue(theValue) ? new ValidationResult(_valueExceededMessage) : ValidationResult.Success;
    }

    private static bool ExceedsMaxCharacterCount(string text) => text.Length > MAX_CHARACTER_COUNT;

    private bool ContainsSpecialCharacters(string text) =>
        _validateSpecialCharacters && SpecialCharRegex().IsMatch(text);

    private bool ContainsNonNumericCharacters(string text) =>
        _validateNonNumericCharacters && AlphaRegex().IsMatch(text);

    private bool ExceedsMaxValue(decimal value) => value > _maxValue;

    private string GetValidationErrorMessage(string text)
    {
        return (ExceedsMaxCharacterCount(text), ContainsSpecialCharacters(text)) switch
        {
            (true, _) => _valueExceededMessage,
            (false, true) => _specialCharactersMessage,
            _ => _nonNumericMessage
        };
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

    [GeneratedRegex(@"[a-zA-Z]")]
    private static partial Regex AlphaRegex();

    [GeneratedRegex(@"[!@#\$%\^&\*\(\)\?:{}|<>/;\[\]'~=`¬]")]
    private static partial Regex SpecialCharRegex();

    [GeneratedRegex(@"^-?(\d{1,3})(,?\d{3})*(\.\d{1,2})?$")]
    private static partial Regex TestRegex();
}
