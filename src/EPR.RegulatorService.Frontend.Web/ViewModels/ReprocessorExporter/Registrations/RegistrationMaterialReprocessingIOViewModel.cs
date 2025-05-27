namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations
{
    using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

    public class RegistrationMaterialReprocessingIOViewModel
    {
        public required Guid RegistrationMaterialId { get; set; }
        public RegistrationMaterialReprocessingIO RegistrationMaterialReprocessingIO { get; set; }
    }
}
