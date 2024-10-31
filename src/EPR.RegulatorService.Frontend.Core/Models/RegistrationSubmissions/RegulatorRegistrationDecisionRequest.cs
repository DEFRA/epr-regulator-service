namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegulatorRegistrationDecisionRequest
{
    public Guid OrganisationId { get; set; }

    public RegulatorDecision Decision { get; set; }

    public Guid SubmissionId { get; set; }

    public string? Comments { get; set; }
}