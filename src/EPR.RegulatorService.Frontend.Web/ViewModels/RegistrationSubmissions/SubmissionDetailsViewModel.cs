namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;

    public class SubmissionDetailsViewModel
    {
        public RegistrationSubmissionStatus Status { get; set; }
        public DateTime DecisionDate { get; set; }

        public DateTime TimeAndDateOfSubmission { get; set; }
        public bool SubmittedOnTime { get; set; }
        public string SubmittedBy { get; set; }
        public string AccountRole { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string DeclaredBy { get; set; }

        // Files with download links
        public List<FileDetails> Files { get; set; }

        public SubmissionDetailsViewModel()
        {
            Files = new List<FileDetails>();
        }
    }

    public class FileDetails
    {
        public string Label { get; set; }
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
    }

}
