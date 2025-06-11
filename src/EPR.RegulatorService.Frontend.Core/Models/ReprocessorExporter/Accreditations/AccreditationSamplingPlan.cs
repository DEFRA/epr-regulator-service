namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;

public class AccreditationSamplingPlan
{
    public required string MaterialName { get; set; }
    public List<AccreditationSamplingPlanFile> Files { get; set; } = [];
}
