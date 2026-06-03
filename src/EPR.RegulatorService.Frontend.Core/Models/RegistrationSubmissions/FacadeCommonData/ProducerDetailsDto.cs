namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

public class ProducerDetailsDto
{
    public string ProducerType { get; set; }

    public int NumberOfOnlineSubsidiaries { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public int NumberOfHoldingCompaniesClosedLoopRecycling { get; set; }

    public int NumberOfSubsidiariesClosedLoopRecycling { get; set; }

    public bool IsProducerOnlineMarketplace { get; set; }

    public int NumberOfSubsidiariesOnlineMarketPlace { get; set; }
}