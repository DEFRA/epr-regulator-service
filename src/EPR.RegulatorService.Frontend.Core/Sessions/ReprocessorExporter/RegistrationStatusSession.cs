using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class RegistrationStatusSession
{
    public Guid RegistrationId { get; init; }
    public required string OrganisationName { get; init; }
    public ApplicationOrganisationType ApplicationType { get; init; }
    public string? SiteAddress { get; init; }
    public required Guid RegistrationMaterialId { get; init; }
    public required string MaterialName { get; init; }
    public decimal FeeAmount { get; init; }
    public string ApplicationReferenceNumber { get; init; }
    public DateTime SubmittedDate { get; init; }
    public string Regulator { get; init; }
    public Guid? RegulatorApplicationTaskStatusId { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }

    public bool? FullPaymentMade { get; set; }
    public PaymentMethodType? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
}