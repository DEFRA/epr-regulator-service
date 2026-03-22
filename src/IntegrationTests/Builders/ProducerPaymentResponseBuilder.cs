namespace IntegrationTests.Builders;

public class ProducerPaymentResponseBuilder
{
    private int _producerRegistrationFee = 165800;
    private readonly int _producerOnlineMarketPlaceFee = 0;
    private readonly int _producerLateRegistrationFee = 0;
    private int _totalFee = 165800;
    private int _previousPayment = 0;
    private int _outstandingPayment = 165800;
    private readonly int _subsidiariesFee = 0;

    private ProducerPaymentResponseBuilder()
    {
    }

    public static ProducerPaymentResponseBuilder Default() => new();

    public ProducerPaymentResponseBuilder WithProducerRegistrationFee(int feeInPence)
    {
        _producerRegistrationFee = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithTotalFee(int feeInPence)
    {
        _totalFee = feeInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithPreviousPayment(int amountInPence)
    {
        _previousPayment = amountInPence;
        return this;
    }

    public ProducerPaymentResponseBuilder WithOutstandingPayment(int amountInPence)
    {
        _outstandingPayment = amountInPence;
        return this;
    }

    public object Build() => new
    {
        producerRegistrationFee = _producerRegistrationFee,
        producerOnlineMarketPlaceFee = _producerOnlineMarketPlaceFee,
        producerLateRegistrationFee = _producerLateRegistrationFee,
        totalFee = _totalFee,
        previousPayment = _previousPayment,
        outstandingPayment = _outstandingPayment,
        subsidiariesFee = _subsidiariesFee,
        subsidiariesFeeBreakdown = new
        {
            totalSubsidiariesOMPFees = 0,
            countOfOMPSubsidiaries = 0
        }
    };
}
