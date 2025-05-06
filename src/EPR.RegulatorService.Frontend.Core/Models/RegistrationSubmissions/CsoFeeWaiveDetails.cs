using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

[ExcludeFromCodeCoverage]
public class CsoFeeWaiveDetails : ProducerFeeWaiveDetails
{
    public int SchemeMemberCount { get; set; }

    public int SmallProducerCount { get; set; }

    public int SmallProducerFee { get; set; }

    public int LargeProducerCount { get; set; }

    public int LargeProducerFee { get; set; }

    public int LateRegistrationCount { get; set; }

    public int LateRegistrationFee { get; set; }
}
