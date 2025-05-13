namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class RegistrationMaterialPaymentFees
{
    public int RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required ApplicationOrganisationType ApplicationType { get; init; }
    public string? SiteAddress { get; init; }
    public required string ApplicationReferenceNumber { get; init; }
    public int RegistrationMaterialId { get; init; }
    public required string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
    public required string Regulator { get; init; }
}
