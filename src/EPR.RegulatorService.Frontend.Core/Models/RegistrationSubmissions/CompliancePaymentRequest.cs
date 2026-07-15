namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

public class CompliancePaymentRequest
{
    public required string ApplicationReferenceNumber { get; set; }

    public string? RegistrationBlobName { get; set; }

    public required string Regulator { get; set; }

    public DateTime SubmissionDate { get; set; }

    public IEnumerable<ComplianceSchemeMemberRequest> ComplianceSchemeMembers { get; set; }
    public bool IncludeRegistrationFee { get; set; } = true;
}

public class ComplianceSchemeMemberRequest
{
    public required string MemberId { get; set; }

    public required string MemberType { get; set; } //"Large" or "Small"

    public bool IsOnlineMarketplace { get; set; }

    public bool IsLateFeeApplicable { get; set; }

    public bool IsClosedLoopRecycling { get; set; }

    public int NoOfHoldingCompaniesClosedLoopRecycling { get; set; }

    public int NoOfSubsidiariesClosedLoopRecycling { get; set; }

    public int NumberOfSubsidiaries { get; set; }

    public int NoOfSubsidiariesOnlineMarketplace { get; set; }
}