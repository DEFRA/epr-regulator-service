namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

using System.Text.Json.Serialization;

public class PackagingProducerPaymentResponse
{
    [JsonPropertyName("previousPayments")]
    public decimal PreviousPaymentsReceived { get; set; }

    [JsonPropertyName("totalResubmissionFee")]
    public decimal ResubmissionFee { get; set; }

    [JsonPropertyName("outstandingPayment")]
    public decimal TotalOutstanding { get; set; }

    public string ReferenceNumber { get; set; }
}