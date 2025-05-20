using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

public abstract class RegistrationStatusViewModelBase
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required ApplicationOrganisationType? ApplicationType { get; init; }
}