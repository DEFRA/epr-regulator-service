namespace EPR.RegulatorService.Frontend.Core.Models;

public class RemoveApprovedUserRequest
{
    public Guid RemovedConnectionExternalId { get; set; }

    public Guid OrganisationId { get; set; }

    public Guid PromotedPersonExternalId { get; set; }
}