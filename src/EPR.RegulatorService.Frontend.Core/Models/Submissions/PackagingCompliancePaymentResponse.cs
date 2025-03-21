namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

[ExcludeFromCodeCoverage]
public class PackagingCompliancePaymentResponse
{        
    [JsonPropertyName("memberCount")]
    public int MemberCount { get; set; }

    [JsonPropertyName("previousPayments")]
    public decimal PreviousPaymentsReceived { get; set; }

    [JsonPropertyName("totalResubmissionFee")]
    public decimal ResubmissionFee { get; set; }

    [JsonPropertyName("outstandingPayment")]
    public decimal TotalOutstanding { get; set; }
}