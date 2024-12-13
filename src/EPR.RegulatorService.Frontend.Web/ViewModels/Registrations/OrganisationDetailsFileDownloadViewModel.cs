namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class OrganisationDetailsFileDownloadViewModel
{
    public bool DownloadFailed { get; set; }
    public bool HasIssue { get; set; }
    public Guid? SubmissionId { get; set; }

    public OrganisationDetailsFileDownloadViewModel() { }

    public OrganisationDetailsFileDownloadViewModel(bool downloadFailed, bool hasIssue, Guid? submissionId = null)
    {
        DownloadFailed = downloadFailed;
        HasIssue = hasIssue;
        SubmissionId = submissionId;
    }
}
