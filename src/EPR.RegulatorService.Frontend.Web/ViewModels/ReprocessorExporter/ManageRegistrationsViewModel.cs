using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

public class ManageRegistrationsViewModel
{
    public int Id { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public string SiteAddress { get; set; } = string.Empty; // Only for reprocessors
    public ApplicationOrganisationType ApplicationOrganisationType { get; set; }
    public string Regulator { get; set; } = "Environment Agency (EA)"; // Default
}