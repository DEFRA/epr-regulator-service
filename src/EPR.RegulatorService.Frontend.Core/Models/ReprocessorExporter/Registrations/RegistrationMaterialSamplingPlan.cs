namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class RegistrationMaterialSamplingPlan
    {
        public required string MaterialName { get; set; }
        public List<RegistrationMaterialSamplingPlanFile> Files { get; set; } = [];
    }
}