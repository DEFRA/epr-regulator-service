using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationAuthorisedMaterials
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public List<MaterialsAuthorisedOnSite> MaterialsAuthorisation { get; set; } = [];
    public Guid? RegulatorRegistrationTaskStatusId { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}
