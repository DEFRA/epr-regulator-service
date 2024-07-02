namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions
{
    [ExcludeFromDescription]
    public class SubmissionListRequest
    {
        public string? SearchOrganisationName { get; set; }
        public string? SearchOrganisationReference { get; set; }
        public bool IsDirectProducerChecked { get; set; }
        public bool IsComplianceSchemeChecked { get; set; }
        public bool IsPendingSubmissionChecked { get; set; }
        public bool IsAcceptedSubmissionChecked { get; set; }
        public bool IsRejectedSubmissionChecked { get; set; }
        public int[] SearchSubmissionYears { get; set; }
        public string[] SearchSubmissionPeriods { get; set; }
        public int[] SubmissionYears { get; set; }
        public string[] SubmissionPeriods { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}