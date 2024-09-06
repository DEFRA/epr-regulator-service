namespace EPR.RegulatorService.Frontend.Core.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ManageUserDetailsChangeRequest
{
    public Guid ChangeHistoryExternalId { get; set; }

    public Guid OrganisationId { get; set; }

    public bool HasRegulatorAccepted { get; set; } = false;

    public string? RegulatorComment { get; set; }
}