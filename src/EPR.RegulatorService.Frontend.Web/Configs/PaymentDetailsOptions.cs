namespace EPR.RegulatorService.Frontend.Web.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class PaymentDetailsOptions
    {
        public const string ConfigSection = "BehaviourManagement:PaymentDetails";

        public bool ShowZeroFeeForTotalOutstanding { get; set; }
    }
}
