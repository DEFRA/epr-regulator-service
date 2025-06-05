using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

public class PaymentMethodViewModel : AccreditationStatusViewModelBase
{
    [Required(ErrorMessage = "Select an option")]
    public PaymentMethodType? PaymentMethod { get; init; }

    public IReadOnlyList<PaymentMethodType> PaymentMethods { get; } = PaymentMethodType.AllTypes;
}