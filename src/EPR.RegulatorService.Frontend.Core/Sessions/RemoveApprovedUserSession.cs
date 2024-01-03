using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class RemoveApprovedUserSession
{
    public string UserNameToDelete { get; set; }
    public Guid ConnExternalId { get; set; }
    public bool? NominationDecision { get; set; }
    public string OrganisationName { get; set; }
    public Guid OrganisationId { get; set; }

    public EndpointResponseStatus? ResponseStatus { get; set; }
}