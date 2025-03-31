using EPR.RegulatorService.Frontend.Core.Enums;

using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ApplicationUpdateViewModel
{
    public string? MaterialName { get; init; }

    [Required(ErrorMessage = "Select an option")]
    public ApplicationStatus? Status { get; init; }
}