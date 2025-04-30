namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class SiteDetails
{
    public int Id { get; init; }
    public string? SiteAddress { get; init; }
    public string? Location { get; init; }
    public string? SiteGridReference { get; init; }
    public string? LegalDocumentAddress { get; init; }
}