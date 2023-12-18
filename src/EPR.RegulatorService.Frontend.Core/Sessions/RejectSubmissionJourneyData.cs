namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class RejectSubmissionJourneyData
{
    public string? OrganisationName { get; set; }
    public Guid SubmissionId { get; set; }
    public string? SubmittedBy { get; set; }
}