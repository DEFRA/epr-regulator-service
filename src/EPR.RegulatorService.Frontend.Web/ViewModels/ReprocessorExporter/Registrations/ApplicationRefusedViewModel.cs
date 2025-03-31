using EPR.RegulatorService.Frontend.Web.Attributes;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using System.ComponentModel.DataAnnotations;

public class ApplicationRefusedViewModel
{
    public string? MaterialName { get; init; }

    [Required(ErrorMessage = "Enter refusal details")]
    [MaxLength(20, ErrorMessage = "Entry exceeds character maximum")]
    public required string Comments { get; init; }
}