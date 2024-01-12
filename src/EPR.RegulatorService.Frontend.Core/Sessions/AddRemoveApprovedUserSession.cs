using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class AddRemoveApprovedUserSession
{
    public string UserNameToRemove { get; set; }
    public Guid ConnExternalId { get; set; }
    public bool? NominationDecision { get; set; }
    public string OrganisationName { get; set; }
    public Guid ExternalOrganisationId { get; set; }
    public EndpointResponseStatus? ResponseStatus { get; set; }
    public OrganisationUser? NewApprovedUser { get; set; }
}