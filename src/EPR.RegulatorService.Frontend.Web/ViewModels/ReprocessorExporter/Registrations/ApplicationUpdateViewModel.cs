using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ApplicationUpdateViewModel
{
    public string? MaterialName { get; set; }

    [Required(ErrorMessage = "Select an option")]
    public ApplicationStatus? Status { get; set; }
}