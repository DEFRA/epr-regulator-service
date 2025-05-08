namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionDetailsFileDownloadViewModel
{
    public bool DownloadFailed { get; set; }
    public bool HasIssue { get; set; }
    public Guid? SubmissionId { get; set; }
    public string SubmittedBy { get; set; }

    public  int SubmissionHash { get; set; }

    public SubmissionDetailsFileDownloadViewModel() { }

    public SubmissionDetailsFileDownloadViewModel(bool downloadFailed, bool hasIssue, Guid? submissionId = null, string? submittedBy="")
    {
        DownloadFailed = downloadFailed;
        HasIssue = hasIssue;
        SubmissionId = submissionId;
        SubmittedBy = submittedBy;
    }
}
