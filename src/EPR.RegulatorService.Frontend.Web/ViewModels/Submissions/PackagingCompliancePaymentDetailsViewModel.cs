using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

[ExcludeFromCodeCoverage]
public class PackagingCompliancePaymentDetailsViewModel : PaymentDetailsViewModel
{
    public decimal ResubmissionFee { get; set; }

    public decimal SubTotal { get; set; }

    public decimal PreviousPaymentReceived { get; set; }

    public decimal TotalOutstanding { get; set; }
}