namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class RegistrationTaskViewModel
{
    public RegulatorTaskType TaskName { get; init; }
    public required string StatusCssClass { get; init; }
    public required string StatusText { get; init; }
}