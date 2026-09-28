namespace IntegrationTests.Builders;

public class ProducerPaymentResponseBuilder
{
    private int _producerRegistrationFee;
    private int _producerLateRegistrationFee;
    private int _producerOnlineMarketPlaceFee = 0;
    private int _producerClosedLoopRecyclingFee;
    private int _previousPayment = 0;
    private int _subsidiariesFee;
    private int _totalFee;
    private int _outstandingPayment;
    private int _ompFee = 0;
    private int _ompCount = 0;
    private int _closedLoopRecyclingFee = 0;
    private int _closedLoopRecyclingCount = 0;
    private string? _producerSize;
    private int _numberOfSubsidiaries;

    private ProducerPaymentResponseBuilder()
    {
    }

    public static ProducerPaymentResponseBuilder Default() => new();

    public ProducerPaymentResponseBuilder WithProducerRegistrationFee(int feeInPence)
    {
        _producerRegistrationFee = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithProducerLateRegistrationFee(int feeInPence)
    {
        _producerLateRegistrationFee = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithProducerClosedLoopRecyclingFee(int feeInPence)
    {
        _producerClosedLoopRecyclingFee = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithTotalFee(int feeInPence)
    {
        _totalFee = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithOutstandingPayment(int feeInPence)
    {
        _outstandingPayment = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithSubsidiariesFeeBreakdown(
        int subsidiariesFeeInPence,
        int closedLoopRecyclingFeeInPence,
        int closedLoopRecyclingCount,
        int ompFeeInPence = 0,
        int ompCount = 0)
    {
        _subsidiariesFee = subsidiariesFeeInPence;
        _closedLoopRecyclingFee = closedLoopRecyclingFeeInPence;
        _closedLoopRecyclingCount = closedLoopRecyclingCount;
        _ompFee = ompFeeInPence;
        _ompCount = ompCount;
        return this;
    }

    public ProducerPaymentResponseBuilder WithProducerSize(string? producerSize)
    {
        _producerSize = producerSize;
        return this;
    }

    public ProducerPaymentResponseBuilder WithNumberOfSubsidiaries(int numberOfSubsidiaries)
    {
        _numberOfSubsidiaries = numberOfSubsidiaries;
        return this;
    }

    public object Build() => new
    {
        producerRegistrationFee = _producerRegistrationFee,
        producerLateRegistrationFee = _producerLateRegistrationFee,
        producerOnlineMarketPlaceFee = _producerOnlineMarketPlaceFee,
        producerClosedLoopRecyclingFee = _producerClosedLoopRecyclingFee,
        previousPayment = _previousPayment,
        subsidiariesFee = _subsidiariesFee,
        totalFee = _totalFee,
        outstandingPayment = _outstandingPayment,
        producerSize = _producerSize,
        numberOfSubsidiaries = _numberOfSubsidiaries,
        subsidiariesFeeBreakdown = new
        {
            totalSubsidiariesOMPFees = _ompFee,
            countOfOMPSubsidiaries = _ompCount,
            totalSubsidiariesClosedLoopRecyclingFees = _closedLoopRecyclingFee,
            countOfClosedLoopRecyclingSubsidiaries = _closedLoopRecyclingCount
        }
    };
}
