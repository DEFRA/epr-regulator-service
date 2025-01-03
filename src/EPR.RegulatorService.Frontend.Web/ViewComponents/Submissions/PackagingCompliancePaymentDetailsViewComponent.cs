using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;

public class PackagingCompliancePaymentDetailsViewComponent(IPaymentFacadeService paymentFacadeService, ILogger<PackagingCompliancePaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(PackagingCompliancePaymentDetailsViewComponent)),
            "An error occurred while retrieving the packaging payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(BaseSubmissionDetailsViewModel viewModel)
    {
        try
        {
            var compliancePaymentResponse = await paymentFacadeService.GetCompliancePaymentDetailsAsync<PackagingCompliancePaymentResponse>(new CompliancePaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                Regulator = viewModel.NationCode,
                ComplianceSchemeMembers = viewModel.CSOMembershipDetails?.Select(x => (ComplianceSchemeMemberRequest)x),
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*payment facade in utc format*/
            });

            if (compliancePaymentResponse is null)
            {
                return View(default(PackagingCompliancePaymentDetailsViewModel));
            }

            var compliancePaymentDetailsViewModel = new PackagingCompliancePaymentDetailsViewModel
            {
                ResubmissionFee = ConvertToPoundsFromPence(compliancePaymentResponse.ResubmissionFee),
                SubTotal = ConvertToPoundsFromPence(compliancePaymentResponse.TotalChargeableItems),
                PreviousPaymentReceived = ConvertToPoundsFromPence(compliancePaymentResponse.PreviousPaymentsReceived),
                TotalOutstanding = ConvertToPoundsFromPence(compliancePaymentResponse.TotalOutstanding),
            };

            return View(compliancePaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
                $"Unable to retrieve the packaging compliance scheme payment details for " +
                $"{viewModel.SubmissionId} in {nameof(PackagingCompliancePaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            return View(default(PackagingCompliancePaymentDetailsViewModel));
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}