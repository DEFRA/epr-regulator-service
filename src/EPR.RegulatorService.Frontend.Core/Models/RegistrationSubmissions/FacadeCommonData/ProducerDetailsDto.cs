namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

public class ProducerDetailsDto
{
    public string ProducerType { get; set; }

    public int NoOfSubsidiariesOnlineMarketPlace { get; set; }

    public int NoOfSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }
}