namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

public class ProducerPaymentResponse
{
    [JsonPropertyName("producerRegistrationFee")]
    public decimal ApplicationProcessingFee { get; set; }

    [JsonPropertyName("producerLateRegistrationFee")]
    public decimal LateRegistrationFee { get; set; }

    [JsonPropertyName("producerOnlineMarketPlaceFee")]
    public decimal OnlineMarketplaceFee { get; set; }

    [JsonPropertyName("previousPayment")]
    public decimal PreviousPaymentsReceived { get; set; }

    [JsonPropertyName("subsidiariesFee")]
    public decimal SubsidiaryFee { get; set; }

    [JsonPropertyName("totalFee")]
    public decimal TotalChargeableItems { get; set; }

    [JsonPropertyName("outstandingPayment")]
    public decimal TotalOutstanding { get; set; }

    public decimal? OfflinePaymentAmount { get; set; } = null!;
}