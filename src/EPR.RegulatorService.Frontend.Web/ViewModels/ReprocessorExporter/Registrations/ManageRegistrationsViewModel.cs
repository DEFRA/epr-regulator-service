using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ManageRegistrationsViewModel
{
    public int Id { get; set; }
    public string OrganisationName { get; init; } = string.Empty;
    public string SiteAddress { get; init; } = string.Empty; // Only for reprocessors
    public ApplicationOrganisationType ApplicationOrganisationType { get; init; }
    public string Regulator { get; init; } = "Environment Agency (EA)"; // Default
}