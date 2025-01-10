namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class RejectSubmissionJourneyData
{
    public string? SubmittedBy { get; set; }
    public bool SubmissionRejected { get; set; } = false;
}