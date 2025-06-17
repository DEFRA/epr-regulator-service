namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialSamplingPlan : RegistrationSectionBase
{
    public Guid RegistrationMaterialId { get; set; }
    public required string MaterialName { get; set; }
    public List<RegistrationMaterialSamplingPlanFile> Files { get; set; } = [];
    public Guid? RegulatorApplicationTaskStatusId { get; init; }
}