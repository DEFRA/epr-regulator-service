namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class RegistrationAuthorisedMaterials
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public List<MaterialsAuthorisedOnSite> MaterialsAuthorisation { get; set; } = [];
    public RegulatorTaskStatus TaskStatus { get; init; }
}
