namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

using System.Text.Json.Serialization;

public class CsoMembershipDetailsDto
{
    public string MemberId { get; set; }
    public string MemberType { get; set; }
    public bool IsOnlineMarketPlace { get; set; }
    public bool IsLateFeeApplicable { get; set; }
    public bool IsClosedLoopRecycling { get; set; }

    public int NumberOfSubsidiariesClosedLoopRecycling { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public int NumberOfSubsidiariesOnlineMarketPlace { get; set; }
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
        NumberOfSubsidiariesClosedLoopRecycling = dto.NumberOfSubsidiariesClosedLoopRecycling,
        NoOfSubsidiariesOnlineMarketplace = dto.NumberOfSubsidiariesOnlineMarketPlace,
        NumberOfSubsidiaries = dto.NumberOfSubsidiaries
    };
}
