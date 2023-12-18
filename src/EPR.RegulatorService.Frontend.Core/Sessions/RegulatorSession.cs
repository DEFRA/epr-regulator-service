using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class RegulatorSession
{
    public List<string> Journey { get; set; } = new();

    public List<RegulatorOrganisation> RegulatorOrganisations { get; set; }

    public Guid? OrganisationId { get; set; }

    public string OrganisationName { get; set; }

    public string ReferenceNumber { get; set; }

    public string SearchOrganisationName { get; set; }

    public bool IsApprovedUserTypeChecked { get; set; }

    public bool IsDelegatedUserTypeChecked { get; set; }

    public AcceptUserJourneyData? AcceptUserJourneyData { get; set; }

    public RejectUserJourneyData? RejectUserJourneyData { get; set; }

    public int? CurrentPageNumber { get; set; }

    public int? RegulatorNation { get; set; }
}
