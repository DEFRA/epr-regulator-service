namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces
{
    public interface ISubmissionDetailsViewModel
    {
        string OrganisationName { get; }
        string SiteAddress { get; }
        string OrganisationType { get; }
        string Regulator { get; }
    }
}
