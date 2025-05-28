using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

public class PaymentCheckViewModel : AccreditationStatusViewModelBase
{
    public decimal FeeAmount { get; init; }
    [Required(ErrorMessage = "Select an option")]
    public bool? FullPaymentMade { get; init; }
}