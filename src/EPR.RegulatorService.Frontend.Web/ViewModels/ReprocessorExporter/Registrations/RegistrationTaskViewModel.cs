using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class RegistrationTaskViewModel
{
    public RegulatorTaskType TaskName { get; init; }
    public required string StatusCssClass { get; init; }
    public required string StatusText { get; init; }
    public required RegulatorTaskStatus TaskStatus { get; init; }
}