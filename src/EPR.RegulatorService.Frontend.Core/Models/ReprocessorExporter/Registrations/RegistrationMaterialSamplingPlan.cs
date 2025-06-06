using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class RegistrationMaterialSamplingPlan
    {
        public Guid RegistrationId { get; set; }
        public required string OrganisationName { get; init; }
        public string? SiteAddress { get; init; }
        public Guid RegistrationMaterialId { get; set; }
        public required string MaterialName { get; set; }
        public List<RegistrationMaterialSamplingPlanFile> Files { get; set; } = [];
        public Guid? RegulatorApplicationTaskStatusId { get; init; }
        public RegulatorTaskStatus TaskStatus { get; init; }
    }
}