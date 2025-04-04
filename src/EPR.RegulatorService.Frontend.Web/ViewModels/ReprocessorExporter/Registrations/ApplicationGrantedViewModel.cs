namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using System.ComponentModel.DataAnnotations;

public class ApplicationGrantedViewModel
{
    public string? MaterialName { get; set; }

    [MaxLength(500, ErrorMessage = "Entry exceeds character maximum")]
    public string? Comments { get; set; }
}