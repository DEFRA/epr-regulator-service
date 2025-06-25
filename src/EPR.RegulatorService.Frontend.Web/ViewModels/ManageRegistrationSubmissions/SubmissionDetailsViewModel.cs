namespace EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions
{
    public class SubmissionDetailsViewModel
    {
        public string TimeAndDateOfSubmission { get; set; }

        public string TimeAndDateOfResubmission { get; set; }

        public string LatestDecisionDate { get; set; }

        public string ResubmissionDecisionDate { get; set; }

        public bool IsSubmittedOnTime { get; set; }

        public string SubmittedBy { get; set; }

        public string AccountRole { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }

        public string DeclaredBy { get; set; }

        public List<FileDetailsViewModel> Files { get; set; } = [];
    }
}
