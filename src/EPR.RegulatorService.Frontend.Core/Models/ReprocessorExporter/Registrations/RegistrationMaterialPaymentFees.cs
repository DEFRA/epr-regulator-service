using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialPaymentFees
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public required ApplicationOrganisationType ApplicationType { get; init; }
    public string? SiteAddress { get; init; }
    public required string ApplicationReferenceNumber { get; init; }
    public Guid RegistrationMaterialId { get; init; }
    public required string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
    public required string Regulator { get; init; }
    public DateTime DulyMadeDate { get; init; }
    public DateTime DeterminationDate { get; init; }
}
