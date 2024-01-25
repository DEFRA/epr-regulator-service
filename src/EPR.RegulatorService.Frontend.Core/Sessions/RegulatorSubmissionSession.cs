using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class RegulatorSubmissionSession
    {
        public List<string> Journey { get; set; } = new();
        public Submission OrganisationSubmission { get; set; }
        public RejectSubmissionJourneyData? RejectSubmissionJourneyData { get; set; }
        public string OrganisationName { get; set; }
        public string? SearchOrganisationName { get; set; } = string.Empty;
        public string? SearchOrganisationId { get; set; } = string.Empty;
        public bool IsDirectProducerChecked { get; set; }
        public bool IsComplianceSchemeChecked { get; set; }
        public bool IsPendingSubmissionChecked { get; set; }
        public bool IsAcceptedSubmissionChecked { get; set; }
        public bool IsRejectedSubmissionChecked { get; set; }
        public int? CurrentPageNumber { get; set; }
    }
}