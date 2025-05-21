using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.ApplicationUpdate;

public class ApplicationUpdateViewModel
{
    public string? MaterialName { get; init; }

    [Required(ErrorMessage = "Select an option")]
    public ApplicationStatus? Status { get; init; }
}