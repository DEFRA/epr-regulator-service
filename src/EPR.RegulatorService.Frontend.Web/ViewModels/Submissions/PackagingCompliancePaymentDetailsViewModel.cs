using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

[ExcludeFromCodeCoverage]
public class PackagingCompliancePaymentDetailsViewModel : PaymentDetailsViewModel
{
    public int MemberCount { get; set; }

    public decimal PreviousPaymentReceived { get; set; }

    public decimal ResubmissionFee { get; set; }

    public decimal TotalOutstanding { get; set; }

    public string NationCode { get; set; }

    public string ReferenceNumber { get; set; }
}