namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

[ExcludeFromCodeCoverage]
public class CompliancePaymentResponse
{
    [JsonPropertyName("complianceSchemeRegistrationFee")]
    public decimal ApplicationProcessingFee { get; set; }

    [JsonPropertyName("previousPayment")]
    public decimal PreviousPaymentsReceived { get; set; }

    [JsonPropertyName("totalFee")]
    public decimal TotalChargeableItems { get; set; }

    [JsonPropertyName("outstandingPayment")]
    public decimal TotalOutstanding { get; set; }

    [JsonPropertyName("complianceSchemeMembersWithFees")]
    public List<ComplianceSchemeMember> ComplianceSchemeMembers { get; set; }

    public decimal? OfflinePaymentAmount { get; set; } = null!;
}

public class ComplianceSchemeMember
{
    [JsonPropertyName("memberId")]
    public string MemberId { get; set; }

    [JsonPropertyName("memberRegistrationFee")]
    public string MemberFee { get; set; }

    [JsonPropertyName("memberOnlineMarketPlaceFee")]
    public string OnlineMarketPlaceFee { get; set; }

    [JsonPropertyName("memberLateRegistrationFee")]
    public decimal LateRegistrationFee { get; set; }

    [JsonPropertyName("subsidiariesFee")]
    public decimal SubsidiaryFee { get; set; }

    [JsonPropertyName("totalMemberFee")]
    public decimal TotalFee { get; set; }

    [JsonPropertyName("subsidiariesFeeBreakdown")]
    public SubsidiariesFeeBreakdown SubsidiariesFeeBreakdown { get; set; }
}

public class SubsidiariesFeeBreakdown
{
    [JsonPropertyName("totalSubsidiariesOMPFees")]
    public decimal TotalSubsidiariesOMPFees { get; set; }

    [JsonPropertyName("countOfOMPSubsidiaries")]
    public int CountOfOMPSubsidiaries { get; set; }

    [JsonPropertyName("unitOMPFees")]
    public int UnitOMPFees { get; set; }

    [JsonPropertyName("feeBreakdowns")]
    public List<FeeBreakdown> FeeBreakdowns { get; set; }
}

public class FeeBreakdown
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