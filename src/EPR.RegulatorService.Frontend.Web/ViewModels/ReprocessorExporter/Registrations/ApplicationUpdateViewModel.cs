using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ApplicationUpdateViewModel
{
    public ApplicationStatus Status { get; init; }

    public string MaterialName { get; init; }
}