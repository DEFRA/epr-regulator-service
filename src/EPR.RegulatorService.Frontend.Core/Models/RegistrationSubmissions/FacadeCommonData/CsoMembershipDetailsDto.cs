namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

using System.Globalization;

public class CsoMembershipDetailsDto
{
    public string MemberId { get; set; }
    public string MemberType { get; set; }
    public bool IsOnlineMarketPlace { get; set; }
    public bool IsLateFeeApplicable { get; set; }
    public int NumberOfSubsidiaries { get; set; }
    public int NoOfSubsidiariesOnlineMarketplace { get; set; }
    public int RelevantYear { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string SubmissionPeriodDescription { get; set; }

    public static implicit operator ComplianceSchemeMemberRequest(CsoMembershipDetailsDto dto) => new ComplianceSchemeMemberRequest
    {
        MemberId = dto.MemberId.ToString(CultureInfo.InvariantCulture),
        MemberType = dto.MemberType,
        IsOnlineMarketplace = dto.IsOnlineMarketPlace,
        IsLateFeeApplicable = dto.IsLateFeeApplicable,
        NoOfSubsidiariesOnlineMarketplace = dto.NumberOfSubsidiaries,
        NumberOfSubsidiaries = dto.NumberOfSubsidiaries
    };
}