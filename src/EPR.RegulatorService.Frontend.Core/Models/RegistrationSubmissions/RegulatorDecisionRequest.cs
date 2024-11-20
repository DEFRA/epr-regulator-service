namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegulatorDecisionRequest
{
    public Guid OrganisationId { get; set; }

    public string Status { get; set; }

    public Guid SubmissionId { get; set; }

    public string? Comments { get; set; } 
    public string CountryName { get; set; }

    public RegistrationSubmissionType RegistrationSubmissionType { get; set; }

    public string? TwoDigitYear { get; set; }

    public string? OrganisationAccountManagementId { get; set; }

    public DateTime? DecisionDate { get; set; }
}