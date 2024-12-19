namespace EPR.RegulatorService.Frontend.Core.Models.Registrations;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class GetRegistrationSubmissionsCsvRequest
{
    public string? SearchOrganisationName { get; set; } = string.Empty;
    public string? SearchOrganisationId { get; set; } = string.Empty;
    public bool IsDirectProducerChecked { get; set; }
    public bool IsComplianceSchemeChecked { get; set; }
    public bool IsPendingRegistrationChecked { get; set; }
    public bool IsAcceptedRegistrationChecked { get; set; }
    public bool IsRejectedRegistrationChecked { get; set; }
    public int[] SearchSubmissionYears { get; set; } = Array.Empty<int>();
    public string[] SearchSubmissionPeriods { get; set; } = Array.Empty<string>();
}
