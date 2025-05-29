namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

public class PaymentDateViewModel : AccreditationStatusViewModelBase
{
    public int? Day { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }
}