namespace EPR.RegulatorService.Frontend.Core.Models;

public class AddRemoveApprovedUserRequest
{
    public Guid OrganisationId { get; set; }
    public Guid? RemovedConnectionExternalId { get; set; }
    public string InvitedPersonEmail { get; set; }

    public string InvitedPersonFirstname { get; set; }

    public string InvitedPersonLastname { get; set; }
}