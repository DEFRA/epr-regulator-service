namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationAuthorisedMaterials
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public List<MaterialsAuthorisedOnSite> MaterialsAuthorisation { get; set; } = [];
}
