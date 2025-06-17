namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class SiteDetails : RegistrationSectionBase
{
    public string? NationName { get; init; }
    public string? GridReference { get; init; }
    public string? LegalCorrespondenceAddress { get; init; }
    public Guid? RegulatorRegistrationTaskStatusId { get; init; }
}