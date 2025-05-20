using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

public class PaymentCheckViewModel : RegistrationStatusViewModelBase
{
    public decimal FeeAmount { get; init; }
    [Required(ErrorMessage = "Select an option")]
    public bool? FullPaymentMade { get; init; }
}