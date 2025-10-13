using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

[ExcludeFromCodeCoverage]
public class ProducerDetailsDto
{
    public string ProducerType { get; set; }

    public int NoOfSubsidiariesOnlineMarketPlace { get; set; }

    public int NoOfSubsidiaries { get; set; }

    public int NumberOfLateSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }
}