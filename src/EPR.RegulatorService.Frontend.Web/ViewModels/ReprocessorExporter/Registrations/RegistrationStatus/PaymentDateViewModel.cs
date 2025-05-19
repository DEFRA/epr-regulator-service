using EPR.RegulatorService.Frontend.Web.Validators;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

[PaymentDateRequired(ErrorMessage = "Enter date in DD MM YYYY format")]
[PaymentDateIsPastDate(ErrorMessage = "Date must be either today or in the past")]
public class PaymentDateViewModel : RegistrationStatusViewModelBase
{
    public int? Day { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }
}