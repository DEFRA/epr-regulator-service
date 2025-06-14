using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class RegistrationMaterialSamplingInspectionViewModel
{
    public Guid RegistrationMaterialId { get; set; }
    public RegistrationMaterialSamplingPlan RegistrationMaterialSamplingPlan { get; set; }
}