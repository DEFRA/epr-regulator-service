using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.ApplicationUpdate;

public class ApplicationGrantedViewModel
{
    public string? MaterialName { get; init; }

    [MaxLength(500, ErrorMessage = "Entry exceeds character maximum")]
    public string? Comments { get; init; }
}