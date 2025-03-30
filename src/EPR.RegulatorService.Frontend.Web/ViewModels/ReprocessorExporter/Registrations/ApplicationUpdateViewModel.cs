using EPR.RegulatorService.Frontend.Core.Enums;

using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ApplicationUpdateViewModel
{
    [Required(ErrorMessage = "Select an option")]
    public ApplicationStatus? Status { get; init; }

    public string MaterialName { get; init; } = string.Empty;
}