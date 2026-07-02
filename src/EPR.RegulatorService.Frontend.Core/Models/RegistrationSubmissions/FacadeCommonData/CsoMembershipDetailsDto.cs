namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

public class CsoMembershipDetailsDto
{
    public string MemberId { get; set; }
    public string MemberType { get; set; }
    public bool IsOnlineMarketPlace { get; set; }
    public bool IsLateFeeApplicable { get; set; }
    public bool IsClosedLoopRecycling { get; set; }
    public int? NumberOfHoldingCompaniesClosedLoopRecycling { get; set; }

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
        NoOfHoldingCompaniesClosedLoopRecycling = dto.IsClosedLoopRecycling ? 1 : 0,
        IsClosedLoopRecycling = dto.IsClosedLoopRecycling,
        NoOfSubsidiariesClosedLoopRecycling = dto.NumberOfSubsidiariesClosedLoopRecycling,
        NoOfSubsidiariesOnlineMarketplace = dto.NumberOfSubsidiariesOnlineMarketPlace,
        NumberOfSubsidiaries = dto.NumberOfSubsidiaries
    };
}
