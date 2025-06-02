using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

public class PaymentReviewViewModel
{
    public Guid RegistrationId { get; init; }
    public required string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public PaymentMethodType? PaymentMethod { get; init; }
    public DateTime? PaymentDate { get; init; }
    public DateTime DeterminationDate { get; set; }
    public int DeterminationWeeks { get; set; }
    public DateTime? DulyMadeDate { get; init; }
}