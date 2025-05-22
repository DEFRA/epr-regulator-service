namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class Accreditation
{
    public int Id { get; set; } // this will be removed

    public Guid IdGuid { get; set; } // this will be replaced

    public string ApplicationReference { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? DeterminationDate { get; set; }

    public int AccreditationYear { get; init; }

    public List<AccreditationTask> Tasks { get; set; } = [];
}