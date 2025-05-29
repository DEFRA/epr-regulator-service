using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

public class AddQueryNoteViewModel
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public string? FormAction { get; set; }
    [Required(ErrorMessage = "Enter query details")]
    public string? Note { get; set; }
}
