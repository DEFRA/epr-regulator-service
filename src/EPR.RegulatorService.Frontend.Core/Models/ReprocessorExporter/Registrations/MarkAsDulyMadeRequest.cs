namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class MarkAsDulyMadeRequest
{
    public DateTime DulyMadeDate { get; init; }
    public DateTime DeterminationDate { get; init; }
}
