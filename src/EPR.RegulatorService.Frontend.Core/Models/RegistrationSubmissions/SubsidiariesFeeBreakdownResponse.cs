namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

public class SubsidiariesFeeBreakdownResponse
{
    [JsonPropertyName("totalSubsidiariesOMPFees")]
    public decimal SubsidiaryOnlineMarketPlaceFee { get; set; }

    [JsonPropertyName("countOfOMPSubsidiaries")]
    public int OnlineMarketPlaceSubsidiariesCount { get; set; }
}