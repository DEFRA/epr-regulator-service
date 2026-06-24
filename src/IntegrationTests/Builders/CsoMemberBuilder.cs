namespace IntegrationTests.Builders;

public class CsoMemberBuilder
{
    private readonly string _memberId;
    private string _memberType = "large";
    private int _numberOfSubsidiaries;
    private int _numberOfHoldingCompaniesClosedLoopRecycling;
    private int _numberOfSubsidiariesClosedLoopRecycling;

    private CsoMemberBuilder(string memberId)
    {
        _memberId = memberId;
    }

    public static CsoMemberBuilder Large(string memberId) => new(memberId);

    public CsoMemberBuilder WithMemberType(string memberType)
    {
        _memberType = memberType;
        return this;
    }

    public CsoMemberBuilder WithSubsidiaries(int count)
    {
        _numberOfSubsidiaries = count;
        return this;
    }

    /// <summary>
    /// Holding-company CLR count on the first CSO member drives how many large members receive the CLR flag.
    /// </summary>
    public CsoMemberBuilder WithHoldingCompanyClrCount(int count)
    {
        _numberOfHoldingCompaniesClosedLoopRecycling = count;
        return this;
    }

    /// <summary>
    /// Facade-only subsidiary CLR count; must not be forwarded to the payment API request.
    /// </summary>
    public CsoMemberBuilder WithSubsidiariesClosedLoopRecycling(int count)
    {
        _numberOfSubsidiariesClosedLoopRecycling = count;
        return this;
    }

    internal object Build(int relevantYear, string submissionDate) => new
    {
        memberId = _memberId,
        memberType = _memberType,
        isOnlineMarketPlace = false,
        isLateFeeApplicable = true,
        numberOfHoldingCompaniesClosedLoopRecycling = _numberOfHoldingCompaniesClosedLoopRecycling,
        NumberOfSubsidiariesClosedLoopRecycling = _numberOfSubsidiariesClosedLoopRecycling,
        numberOfSubsidiaries = _numberOfSubsidiaries,
        NumberOfSubsidiariesOnlineMarketPlace = 0,
        relevantYear,
        submittedDate = submissionDate,
        submissionPeriodDescription = $"January to December {relevantYear}"
    };
}
