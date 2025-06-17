namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationAuthorisedMaterials : RegistrationSectionBase
{
    public List<MaterialsAuthorisedOnSite> MaterialsAuthorisation { get; set; } = [];
    public Guid? RegulatorRegistrationTaskStatusId { get; init; }
}
