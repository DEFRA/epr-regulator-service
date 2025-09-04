namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class UpdateMaterialTaskStatusRequest
    {
        public string TaskName { get; set; }                 
        public Guid RegistrationMaterialId { get; set; }
        public string Status { get; set; }              
        public string? Comments { get; set; }

    }
}
