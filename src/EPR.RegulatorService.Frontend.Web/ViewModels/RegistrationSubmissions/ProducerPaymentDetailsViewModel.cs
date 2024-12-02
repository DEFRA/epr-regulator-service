namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System;
using System.Globalization;

using EPR.RegulatorService.Frontend.Web.Attributes;

public class ProducerPaymentDetailsViewModel
{
    public decimal ApplicationProcessingFee { get; set; }

    public decimal LateRegistrationFee { get; set; }

    public decimal OnlineMarketplaceFee { get; set; }

    public decimal SubsidiaryFee { get; set; }

    public decimal SubsidiaryOnlineMarketPlaceFee { get; set; }

    public decimal TotalChargeableItems { get; set; }

    public decimal PreviousPaymentsReceived { get; set; }

    public decimal TotalOutstanding { get; set; }

    public string ProducerSize { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public int NumberOfSubsidiariesBeingOnlineMarketplace { get; set; }

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