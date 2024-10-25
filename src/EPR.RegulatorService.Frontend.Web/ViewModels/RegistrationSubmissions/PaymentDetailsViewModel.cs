namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;

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

        [CurrencyValidation(requiredErrorMessage:"PaymentValidation.TheAmountIsRequired",
                            invalidFormatMessage:"PaymentValidation.FormatIsInvalid",
                            valueExceededMessage:"PaymentValidation.ValueTooHigh",
                            maxValue:"10000000.00M",
                            specialCharactersMessage:"PaymentValidation.InvalidCharacters",
                            nonNumericMessage:"PaymentValidation.NonNumericCharacters"
                            )]
        public string? OfflinePayment { get; set; }
    }
}
