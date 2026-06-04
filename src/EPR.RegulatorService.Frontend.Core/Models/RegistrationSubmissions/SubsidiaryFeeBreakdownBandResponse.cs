namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

public class SubsidiaryFeeBreakdownBandResponse
{
    [JsonPropertyName("bandNumber")]
    public int BandNumber { get; set; }

    [JsonPropertyName("unitCount")]
    public int UnitCount { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }
}
