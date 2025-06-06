namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;

public class UpdateAccreditationTaskStatusRequest
{
    public string TaskName { get; set; }
    public Guid AccreditationId { get; set; }
    public string Status { get; set; }
    public string? Comments { get; set; }
}
