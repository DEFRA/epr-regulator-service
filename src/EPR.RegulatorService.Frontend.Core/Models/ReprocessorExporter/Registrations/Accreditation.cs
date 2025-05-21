namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class Accreditation
{
    public int AccreditationId { get; set; }
    public string ApplicationReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DeterminationDate { get; set; }
    public List<AccreditationTask> Tasks { get; set; } = [];
}