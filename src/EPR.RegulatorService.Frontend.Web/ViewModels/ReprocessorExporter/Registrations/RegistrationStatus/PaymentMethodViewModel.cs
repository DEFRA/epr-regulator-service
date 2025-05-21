using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

public class PaymentMethodViewModel : RegistrationStatusViewModelBase
{
    [Required(ErrorMessage = "Select an option")]
    public PaymentMethodType? PaymentMethod { get; init; }

    public IReadOnlyList<PaymentMethodType> PaymentMethods { get; } = PaymentMethodType.AllTypes;
}