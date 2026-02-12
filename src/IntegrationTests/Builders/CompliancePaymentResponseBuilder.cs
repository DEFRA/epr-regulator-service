namespace IntegrationTests.Builders;

public class CompliancePaymentResponseBuilder
{
    private int _complianceSchemeRegistrationFee = 100000;
    private int _totalFee = 100000;
    private int _previousPayment = 0;
    private int _outstandingPayment = 100000;
    private readonly List<ComplianceSchemeMemberFeeBuilder> _members = new();

    private CompliancePaymentResponseBuilder()
    {
        // Include a default member so the response is valid
        _members.Add(ComplianceSchemeMemberFeeBuilder.Default());
    }

    public static CompliancePaymentResponseBuilder Default() => new();

    public CompliancePaymentResponseBuilder WithComplianceSchemeRegistrationFee(int feeInPence)
    {
        _complianceSchemeRegistrationFee = feeInPence;
        return this;
    }

    public CompliancePaymentResponseBuilder WithTotalFee(int feeInPence)
    {
        _totalFee = feeInPence;
        return this;
    }

    public CompliancePaymentResponseBuilder WithPreviousPayment(int amountInPence)
    {
        _previousPayment = amountInPence;
        return this;
    }

    public CompliancePaymentResponseBuilder WithOutstandingPayment(int amountInPence)
    {
        _outstandingPayment = amountInPence;
        return this;
    }

    public CompliancePaymentResponseBuilder WithMember(ComplianceSchemeMemberFeeBuilder member)
    {
        _members.Add(member);
        return this;
    }

    public object Build() => new
    {
        complianceSchemeRegistrationFee = _complianceSchemeRegistrationFee,
        totalFee = _totalFee,
        previousPayment = _previousPayment,
        outstandingPayment = _outstandingPayment,
        complianceSchemeMembersWithFees = _members.Select(m => m.Build()).ToArray()
    };
}

public class ComplianceSchemeMemberFeeBuilder
{
    private string _memberId = "100001";
    private string _memberType = "large";
    private int _memberRegistrationFee = 165800;
    private int _memberOnlineMarketPlaceFee = 0;
    private int _memberLateRegistrationFee = 0;
    private int _subsidiariesFee = 0;
    private int _totalMemberFee = 165800;

    private ComplianceSchemeMemberFeeBuilder()
    {
    }

    public static ComplianceSchemeMemberFeeBuilder Default() => new();

    public ComplianceSchemeMemberFeeBuilder WithMemberId(string memberId)
    {
        _memberId = memberId;
        return this;
    }

    public ComplianceSchemeMemberFeeBuilder WithMemberType(string memberType)
    {
        _memberType = memberType;
        return this;
    }

    public ComplianceSchemeMemberFeeBuilder WithMemberRegistrationFee(int feeInPence)
    {
        _memberRegistrationFee = feeInPence;
        return this;
    }

    public ComplianceSchemeMemberFeeBuilder WithOnlineMarketPlaceFee(int feeInPence)
    {
        _memberOnlineMarketPlaceFee = feeInPence;
        return this;
    }

    public ComplianceSchemeMemberFeeBuilder WithLateRegistrationFee(int feeInPence)
    {
        _memberLateRegistrationFee = feeInPence;
        return this;
    }

    public ComplianceSchemeMemberFeeBuilder WithSubsidiariesFee(int feeInPence)
    {
        _subsidiariesFee = feeInPence;
        return this;
    }

    public ComplianceSchemeMemberFeeBuilder WithTotalMemberFee(int feeInPence)
    {
        _totalMemberFee = feeInPence;
        return this;
    }

    public object Build() => new
    {
        memberId = _memberId,
        memberType = _memberType,
        memberRegistrationFee = _memberRegistrationFee,
        memberOnlineMarketPlaceFee = _memberOnlineMarketPlaceFee,
        memberLateRegistrationFee = _memberLateRegistrationFee,
        subsidiariesFee = _subsidiariesFee,
        totalMemberFee = _totalMemberFee
    };
}
