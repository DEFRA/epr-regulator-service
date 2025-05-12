using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public class RegulatorSubmissionSession
    {
        public RegulatorSubmissionSession()
        {
            OrganisationSubmissions = new Dictionary<int, Submission>();
        }

        public List<string> Journey { get; set; } = new();
        public IDictionary<int, Submission> OrganisationSubmissions { get; set; }
        public RejectSubmissionJourneyData? RejectSubmissionJourneyData { get; set; }
        public string? SearchOrganisationName { get; set; } = string.Empty;
        public string? SearchOrganisationId { get; set; } = string.Empty;
        public bool IsDirectProducerChecked { get; set; }
        public bool IsComplianceSchemeChecked { get; set; }
        public bool IsPendingSubmissionChecked { get; set; }
        public bool IsAcceptedSubmissionChecked { get; set; }
        public bool IsRejectedSubmissionChecked { get; set; }
        public int[] SearchSubmissionYears { get; set; }
        public string[] SearchSubmissionPeriods { get; set; }
        public int? CurrentPageNumber { get; set; }

        public static int GetSubmissionHashCode(Submission submission) => submission is null
        ? throw new ArgumentNullException(nameof(submission))
        : HashCode.Combine(submission.SubmissionId, submission.SubmittedDate) & int.MaxValue;

    }
}