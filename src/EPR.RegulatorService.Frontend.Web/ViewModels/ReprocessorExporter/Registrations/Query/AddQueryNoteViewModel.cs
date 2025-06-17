using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

public class AddQueryNoteViewModel
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public RegulatorTaskStatus? TaskStatus { get; init; }
    public string? FormAction { get; set; }
    [Required(ErrorMessage = "Enter query details")]
    [MaxLength(500, ErrorMessage = "Entry exceeds character maximum")]
    public string? Note { get; set; }
}
