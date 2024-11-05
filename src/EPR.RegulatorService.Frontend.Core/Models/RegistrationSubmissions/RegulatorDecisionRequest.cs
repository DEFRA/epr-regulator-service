namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class RegulatorDecisionRequest
{
    public Guid OrganisationId { get; set; }

    public string Decision { get; set; }

    public Guid SubmissionId { get; set; }

    public string? Comments { get; set; }
}