namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;

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

        public static implicit operator RegistrationSubmissionsOrganisationPaymentDetails(PaymentDetailsViewModel details) => details is null ? null : new RegistrationSubmissionsOrganisationPaymentDetails
        {
            ApplicationProcessingFee = details.ApplicationProcessingFee,
            OnlineMarketplaceFee = details.OnlineMarketplaceFee,
            PreviousPaymentsReceived = details.PreviousPaymentsReceived,
            SubsidiaryFee = details.SubsidiaryFee
        };

        public static implicit operator PaymentDetailsViewModel(RegistrationSubmissionsOrganisationPaymentDetails details) => details is null ? null : new()
        {
            ApplicationProcessingFee = details.ApplicationProcessingFee,
            OnlineMarketplaceFee = details.OnlineMarketplaceFee,
            PreviousPaymentsReceived = details.PreviousPaymentsReceived,
            SubsidiaryFee = details.SubsidiaryFee
        };

    }
}
