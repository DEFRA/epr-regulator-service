using EPR.RegulatorService.Frontend.Core.Models;
namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class InviteNewApprovedPersonSession
{
    public string UserNameToRemove { get; set; }
    public Guid? RemovedConnectionExternalId { get; set; }
    public bool? HasANewApprovedPersonBeenNominated { get; set; }
    public string OrganisationName { get; set; }
    public Guid ExternalOrganisationId { get; set; }
    public string InvitedPersonFirstname { get; set; }
    public string InvitedPersonLastname { get; set; }
    public string InvitedPersonEmail { get; set; }
    public string InvitedPersonFullName => string.Join(" ", InvitedPersonFirstname, InvitedPersonLastname);
    public EndpointResponseStatus? ResponseStatus { get; set; }
}