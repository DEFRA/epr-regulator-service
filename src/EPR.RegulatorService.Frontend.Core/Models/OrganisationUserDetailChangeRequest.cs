namespace EPR.RegulatorService.Frontend.Core.Models;

public class OrganisationUserDetailChangeRequest
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public Guid ChangeHistoryExternalId { get; set; }
    public DateTime DeclarationDate { get; set; }
    public string? ServiceRole { get; set; } = string.Empty;
    public int? ServiceRoleId { get; set; }
}