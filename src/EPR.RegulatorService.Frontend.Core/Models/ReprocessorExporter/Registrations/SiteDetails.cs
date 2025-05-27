namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class SiteDetails
{
    public Guid RegistrationId { get; init; }
    public string? SiteAddress { get; init; }
    public string? NationName { get; init; }
    public string? GridReference { get; init; }
    public string? LegalCorrespondenceAddress { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}