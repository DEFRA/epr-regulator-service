namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class UpdateRegistrationTaskStatusRequest
    {
        public string TaskName { get; set; }
        public int RegistrationId { get; set; }
        public string Status { get; set; }
        public string? Comments { get; set; }

    }
}
