using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class SiteDetails
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public string? NationName { get; init; }
    public string? GridReference { get; init; }
    public string? LegalCorrespondenceAddress { get; init; }
    public Guid? RegulatorRegistrationTaskStatusId { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}