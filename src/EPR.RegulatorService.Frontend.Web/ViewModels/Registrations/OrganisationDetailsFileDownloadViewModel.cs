namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class OrganisationDetailsFileDownloadViewModel
{
    public bool IsFileDownloading { get; set; }
    public bool DownloadFailed { get; set; }
    public bool HasVirus { get; set; }
}
