using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;

public class OfflinePaymentRequest
{
    public decimal Amount { get; init; }
    public string PaymentReference { get; init; }
    public DateTime PaymentDate { get; init; }
    public PaymentMethodType PaymentMethod { get; init; }
    public string Regulator { get; init; }
}
