namespace EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public sealed class PaymentMethodType : StringEnumBase
{
    private PaymentMethodType(string value) : base(value) { }

    public static readonly PaymentMethodType BankTransfer = new("Bank transfer (Bacs)");
    public static readonly PaymentMethodType CreditOrDebitCard = new("Credit or debit card");
    public static readonly PaymentMethodType Cheque = new("Cheque");
    public static readonly PaymentMethodType Cash = new("Cash");

    public static PaymentMethodType[] AllTypes =>
    [
        BankTransfer,
        CreditOrDebitCard,
        Cheque,
        Cash
    ];
}