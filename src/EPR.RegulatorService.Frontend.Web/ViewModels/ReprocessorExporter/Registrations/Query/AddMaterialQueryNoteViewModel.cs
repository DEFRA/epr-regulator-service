using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

public class AddMaterialQueryNoteViewModel
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public ApplicationOrganisationType? ApplicationType { get; init; }
    [Required(ErrorMessage = "Enter query details")]
    public string? Note { get; set; }
}
