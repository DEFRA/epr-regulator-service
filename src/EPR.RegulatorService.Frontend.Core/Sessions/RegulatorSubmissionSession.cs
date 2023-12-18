using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class RegulatorSubmissionSession
    {
        public List<string> Journey { get; set; } = new();
        public Submission OrganisationSubmission { get; set; }
        public RejectSubmissionJourneyData? RejectSubmissionJourneyData { get; set; }
        public SubmissionFiltersModel? SubmissionFiltersModel { get; set; } = new();
        public int? PageNumber { get; set; }
    }
}