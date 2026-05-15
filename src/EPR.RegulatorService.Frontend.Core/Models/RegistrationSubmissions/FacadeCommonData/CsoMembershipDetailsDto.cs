namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

using System.Text.Json.Serialization;

public class CsoMembershipDetailsDto
{
    public string MemberId { get; set; }
    public string MemberType { get; set; }
    public bool IsOnlineMarketPlace { get; set; }
    public bool IsLateFeeApplicable { get; set; }
    public bool IsClosedLoopRecycling { get; set; }

    [JsonPropertyName("NumberOfSubsidiariesClosedLoopRecycling")]
    public int NoOfSubsidiariesClosedLoopRecycling { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    [JsonPropertyName("NumberOfSubsidiariesOnlineMarketPlace")]
    public int NoOfSubsidiariesOnlineMarketplace { get; set; }
    public int RelevantYear { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string SubmissionPeriodDescription { get; set; }

    public static implicit operator ComplianceSchemeMemberRequest(CsoMembershipDetailsDto dto) => new()
    {
        MemberId = dto.MemberId,
        MemberType = dto.MemberType,
        IsOnlineMarketplace = dto.IsOnlineMarketPlace,
        IsLateFeeApplicable = dto.IsLateFeeApplicable,
        IsClosedLoopRecycling = dto.IsClosedLoopRecycling,
        NoOfSubsidiariesClosedLoopRecycling = dto.NoOfSubsidiariesClosedLoopRecycling,
        NoOfSubsidiariesOnlineMarketplace = dto.NoOfSubsidiariesOnlineMarketplace,
        NumberOfSubsidiaries = dto.NumberOfSubsidiaries
    };
}
