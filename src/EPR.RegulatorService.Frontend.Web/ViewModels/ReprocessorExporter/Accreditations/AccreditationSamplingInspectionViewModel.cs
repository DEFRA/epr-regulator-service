namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;

public class AccreditationSamplingInspectionViewModel
{
    public Guid AccreditationId { get; set; }
    public int Year { get; set; }
    public AccreditationSamplingPlan AccreditationSamplingPlan { get; set; }
}