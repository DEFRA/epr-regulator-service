namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public abstract class RegistrationSectionBase
{
    public Guid RegistrationId { get; init; }
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}
