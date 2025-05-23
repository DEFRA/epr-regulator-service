namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class AddMaterialQueryNoteViewModel
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public ApplicationOrganisationType? ApplicationType { get; init; }
    public required string Note { get; set; }
}
