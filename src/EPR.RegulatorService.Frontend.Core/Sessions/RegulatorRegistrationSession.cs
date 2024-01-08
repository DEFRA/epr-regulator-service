using EPR.RegulatorService.Frontend.Core.Models.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class RegulatorRegistrationSession
{
    public List<string> Journey { get; set; } = new();
    public Registration OrganisationRegistration { get; set; }
    public RejectRegistrationJourneyData? RejectRegistrationJourneyData { get; set; }
    public RegistrationFiltersModel? RegistrationFiltersModel { get; set; } = new();
    public int? PageNumber { get; set; }
}
