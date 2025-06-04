using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

public class AddQueryNoteViewModel
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public string? FormAction { get; set; }
    [Required(ErrorMessage = "Enter query details")]
    [MaxLength(500, ErrorMessage = "Comments must be 500 characters or less")]
    public string? Note { get; set; }
}
