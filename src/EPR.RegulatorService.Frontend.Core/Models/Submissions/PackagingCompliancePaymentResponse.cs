namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

[ExcludeFromCodeCoverage]
public class PackagingCompliancePaymentResponse
{
    [JsonPropertyName("resubmissionFee")]
    public decimal ResubmissionFee { get; set; }

    [JsonPropertyName("previousPayment")]
    public decimal PreviousPaymentsReceived { get; set; }

    [JsonPropertyName("totalFee")]
    public decimal TotalChargeableItems { get; set; }

    [JsonPropertyName("outstandingPayment")]
    public decimal TotalOutstanding { get; set; }
}