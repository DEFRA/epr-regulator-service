using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

public class PaymentReviewViewModel
{
    public required string MaterialName { get; init; }
    public DateTime SubmittedDate { get; init; }
    public PaymentMethodType? PaymentMethod { get; init; }
    public DateTime? PaymentDate { get; init; }
    public DateTime DeterminationDate { get; set; }
    public int DeterminationWeeks { get; set; }
}