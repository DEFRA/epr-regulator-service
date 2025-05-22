namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

    public class RegistrationMaterialSamplingPlan
    {
        public required string MaterialName { get; set; }
        public List<RegistrationMaterialSamplingPlanFile> Files { get; set; } = [];
        public RegulatorTaskStatus TaskStatus { get; init; }

    }
}