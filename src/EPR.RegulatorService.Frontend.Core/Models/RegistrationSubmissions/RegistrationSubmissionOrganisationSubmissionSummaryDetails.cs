namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegistrationSubmissionOrganisationSubmissionSummaryDetails
{
    public class FileDetails
    {
        public string Label { get; set; }
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
    }

    public RegistrationSubmissionStatus Status { get; set; }
    public DateTime DecisionDate { get; set; }

    public DateTime TimeAndDateOfSubmission { get; set; }
    public bool SubmittedOnTime { get; set; }
    public string SubmittedBy { get; set; }
    public ServiceRole AccountRole { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string DeclaredBy { get; set; }

    public List<FileDetails> Files { get; set; } = [];
}