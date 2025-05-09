namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialPaymentFees
{
    public string OrganisationName { get; init; }
    public string SiteAddress { get; init; }
    public string ApplicationReferenceNumber { get; init; }
    public int RegistrationMaterialId { get; init; }
    public string? MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public decimal FeeAmount { get; init; }
}
