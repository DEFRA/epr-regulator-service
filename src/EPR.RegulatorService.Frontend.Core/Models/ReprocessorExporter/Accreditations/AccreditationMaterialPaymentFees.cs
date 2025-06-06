using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;

public class AccreditationMaterialPaymentFees
{
    public Guid AccreditationId { get; init; }
    public required string OrganisationName { get; init; }
    public required ApplicationOrganisationType ApplicationType { get; init; }
    public string? SiteAddress { get; init; }
    public required string ApplicationReferenceNumber { get; init; }
    public required string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
    public PrnTonnageType PrnTonnage { get; set; }
    public required string Regulator { get; init; }
}
