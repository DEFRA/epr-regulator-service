namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class OfflinePaymentRequest
{
    public required Guid UserId { get; set; }

    public required string Reference { get; set; }

    public required string Regulator { get; set; }

    public required int Amount { get; set; }

    public required string Description { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Comments { get; set; }
}