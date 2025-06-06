using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

public abstract class AccreditationStatusViewModelBase
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required ApplicationOrganisationType? ApplicationType { get; init; }
}