namespace EPR.RegulatorService.Frontend.Core.Models;

public class RemoveApprovedUserRequest
{
    public Guid ConnectionExternalId { get; set; }

    public Guid OrganisationId { get; set; }

    public bool NominationDecision { get; set; }
}