namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

public class SubsidiariesFeeBreakdownResponse
{
    [JsonPropertyName("totalSubsidiariesOMPFees")]
    public decimal SubsidiaryOnlineMarketPlaceFee { get; set; }

    [JsonPropertyName("countOfOMPSubsidiaries")]
    public int OnlineMarketPlaceSubsidiariesCount { get; set; }

    [JsonPropertyName("totalSubsidiariesClosedLoopRecyclingFees")]
    public decimal SubsidiaryClosedLoopRecyclingFee { get; set; }

    [JsonPropertyName("countOfClosedLoopRecyclingSubsidiaries")]
    public int ClosedLoopRecyclingSubsidiariesCount { get; set; }

    [JsonPropertyName("unitClosedLoopRecyclingFees")]
    public decimal UnitClosedLoopRecyclingFee { get; set; }
}