namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Enums;

[ExcludeFromCodeCoverage]
public class OrganisationDetailsFileDownloadViewModel
{
    public bool DownloadFailed { get; set; }
    public bool HasIssue { get; set; }
    public Guid? SubmissionId { get; set; }
    public RegistrationSubmissionOrganisationType? OrganisationType { get; set; }

    public OrganisationDetailsFileDownloadViewModel() { }

    public OrganisationDetailsFileDownloadViewModel(bool downloadFailed, bool hasIssue, RegistrationSubmissionOrganisationType? organisationType = null, Guid? submissionId = null)
    {
        DownloadFailed = downloadFailed;
        HasIssue = hasIssue;
        SubmissionId = submissionId;
        OrganisationType = organisationType;
    }
}
