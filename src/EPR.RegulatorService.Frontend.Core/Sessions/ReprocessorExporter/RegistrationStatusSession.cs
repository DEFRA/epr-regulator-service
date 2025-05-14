namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

using Enums.ReprocessorExporter;

public class RegistrationStatusSession
{
    public int RegistrationId { get; set; }
    public required string OrganisationName { get; init; }
    public ApplicationOrganisationType ApplicationType { get; init; }
    public required string SiteAddress { get; init; }
    public required int RegistrationMaterialId { get; init; }
    public required string MaterialName { get; init; }
    public decimal FeeAmount { get; init; }
    public string ApplicationReferenceNumber { get; init; }
    public DateTime SubmittedDate { get; init; }
    public string Regulator { get; init; }
    public bool? FullPaymentMade { get; set; }
    public PaymentMethodType? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
}