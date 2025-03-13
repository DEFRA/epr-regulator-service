using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

public sealed class ManageApplicationsViewModel
{
    public ApplicationType ApplicationType { get; init; }

    public ApplicationOrganisationType ApplicationOrganisationType { get; init; }

    public string Title => ApplicationType == ApplicationType.Registration ? "Manage Registrations" : "Manage Accreditations";
}