namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Attributes;

    [ExcludeFromCodeCoverage]
    public class PaymentDetailsViewModel
    {
        public decimal ApplicationProcessingFee { get; set; }

        public decimal OnlineMarketplaceFee { get; set; }

        public decimal SubsidiaryFee { get; set; }

        public decimal TotalChargeableItems => ApplicationProcessingFee + OnlineMarketplaceFee + SubsidiaryFee;

        public decimal PreviousPaymentsReceived { get; set; }

        public decimal TotalOutstanding => TotalChargeableItems - PreviousPaymentsReceived;

        [CurrencyValidation(requiredErrorMessage: "PaymentValidation.TheAmountIsRequired",
                            invalidFormatMessage: "PaymentValidation.FormatIsInvalid",
                            valueExceededMessage: "PaymentValidation.ValueIsTooHigh",
                            maxValue: "10000000.00",
                            specialCharactersMessage: "PaymentValidation.InvalidCharacters",
                            nonNumericMessage: "PaymentValidation.NonNumericCharacters"
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

        public static implicit operator RegistrationSubmissionsOrganisationPaymentDetails(PaymentDetailsViewModel details)
        {
            if (details == null)
            {
                return null;
            }

            return new RegistrationSubmissionsOrganisationPaymentDetails
            {
                ApplicationProcessingFee = details.ApplicationProcessingFee,
                OnlineMarketplaceFee = details.OnlineMarketplaceFee,
                PreviousPaymentsReceived = details.PreviousPaymentsReceived,
                SubsidiaryFee = details.SubsidiaryFee,
                OfflinePaymentAmount = string.IsNullOrEmpty(details.OfflinePayment) ? null : decimal.Parse(details.OfflinePayment, CultureInfo.CurrentCulture)
            };
        }

        public static implicit operator PaymentDetailsViewModel(RegistrationSubmissionsOrganisationPaymentDetails details) => details is null ? null : new()
        {
            ApplicationProcessingFee = details.ApplicationProcessingFee,
            OnlineMarketplaceFee = details.OnlineMarketplaceFee,
            PreviousPaymentsReceived = details.PreviousPaymentsReceived,
            SubsidiaryFee = details.SubsidiaryFee,
            OfflinePayment = string.Format(CultureInfo.InvariantCulture, "{0:F2}", details.OfflinePaymentAmount)
        };
    }
}
