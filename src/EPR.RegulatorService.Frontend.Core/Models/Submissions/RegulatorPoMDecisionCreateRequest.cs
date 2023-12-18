namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

using Enums;

public class RegulatorPoMDecisionCreateRequest
{
    public Guid SubmissionId { get; set; }

    public RegulatorDecision Decision { get; set; }

    public string? Comments { get; set; }

    public bool? IsResubmissionRequired { get; set; }

    public Guid FileId { get; set; }

    public Guid OrganisationId { get; set; }

    public string OrganisationNumber { get; set; }

    public string OrganisationName { get; set; }
}