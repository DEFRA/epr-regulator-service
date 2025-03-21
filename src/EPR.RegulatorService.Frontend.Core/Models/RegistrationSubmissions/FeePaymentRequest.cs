namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class FeePaymentRequest
{
    public required Guid SubmissionId { get; set; }

    public required string PaymentMethod { get; set; }

    public required string PaymentStatus { get; set; }

    public required string PaidAmount { get; set; }

    public required Guid UserId { get; set; }
}