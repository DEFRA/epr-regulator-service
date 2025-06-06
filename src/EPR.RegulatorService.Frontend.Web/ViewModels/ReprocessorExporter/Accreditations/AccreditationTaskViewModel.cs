namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class AccreditationTaskViewModel
{
    public RegulatorTaskType TaskName { get; init; }

    public string StatusCssClass { get; init; } = "govuk-tag--grey";

    public string StatusText { get; init; } = "Not started yet";
}