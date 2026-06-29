namespace IntegrationTests.Builders;

public class CsoMemberBuilder
{
    private readonly string _memberId;
    private string _memberType = "large";
    private int _numberOfSubsidiaries;
    private bool _isClosedLoopRecycling;
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

    public CsoMemberBuilder WithIsClosedLoopRecycling(bool isClosedLoopRecycling = true)
    {
        _isClosedLoopRecycling = isClosedLoopRecycling;
        return this;
    }

    /// <summary>
    /// Subsidiary CLR count from facade data; forwarded to the payment API request per member.
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
        isClosedLoopRecycling = _isClosedLoopRecycling,
        numberOfSubsidiariesClosedLoopRecycling = _numberOfSubsidiariesClosedLoopRecycling,
        numberOfSubsidiaries = _numberOfSubsidiaries,
        NumberOfSubsidiariesOnlineMarketPlace = 0,
        relevantYear,
        submittedDate = submissionDate,
        submissionPeriodDescription = $"January to December {relevantYear}"
    };
}
