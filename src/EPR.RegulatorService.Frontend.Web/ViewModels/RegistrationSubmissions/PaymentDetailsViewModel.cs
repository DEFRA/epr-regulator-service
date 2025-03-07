namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using EPR.RegulatorService.Frontend.Web.Attributes;

[ExcludeFromCodeCoverage]
public class PaymentDetailsViewModel
{
    [CurrencyValidation(requiredErrorMessage: "PaymentValidation.TheAmountIsRequired",
                        invalidFormatMessage: "PaymentValidation.FormatIsInvalid",
                        valueExceededMessage: "PaymentValidation.ValueIsTooHigh",
                        maxValue: "10000000.00",
                        specialCharactersMessage: "PaymentValidation.InvalidCharacters",
                        nonNumericMessage: "PaymentValidation.NonNumericCharacters",
                        valueZeroMesssage: "PaymentValidation.ValueCannotBeZero"
                        )]
    public string? OfflinePayment { get; set; }

    public void EnsureTwoDecimalPlaces()
    {
        if (decimal.TryParse(OfflinePayment, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal parsedValue))
        {
            // Format the parsed value as a currency with two decimal places
            OfflinePayment = parsedValue.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}
