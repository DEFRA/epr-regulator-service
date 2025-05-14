namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

using Core.Enums.ReprocessorExporter;

public abstract class RegistrationStatusViewModelBase
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required ApplicationOrganisationType? ApplicationType { get; init; }
}