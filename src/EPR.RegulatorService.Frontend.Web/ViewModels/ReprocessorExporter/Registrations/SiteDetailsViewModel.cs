namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class SiteDetailsViewModel
{
    public required int RegistrationId { get; init; }
    public required string Location { get; init; }
    public required string SiteAddress { get; init; }
    public required string SiteGridReference { get; init; }
    public required string LegalDocumentAddress { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }

}