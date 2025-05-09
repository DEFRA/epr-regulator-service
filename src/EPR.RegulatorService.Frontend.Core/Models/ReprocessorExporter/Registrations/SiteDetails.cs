namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class SiteDetails
{
    public int Id { get; init; }
    public string? SiteAddress { get; init; }
    public string? NationName { get; init; }
    public string? GridReference { get; init; }
    public string? LegalCorrespondenceAddress { get; init; }
}