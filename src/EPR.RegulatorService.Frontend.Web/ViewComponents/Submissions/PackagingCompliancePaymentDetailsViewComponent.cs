using System.Threading.Tasks;

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
            if (viewModel.ReferenceFieldNotAvailable)
            {
                this.ViewBag.NoReferenceField = this.ViewBag.NoReferenceNumber = true;
                return View(default(PackagingCompliancePaymentDetailsViewModel));
            }

            if (viewModel.ReferenceNotAvailable)
            {
                this.ViewBag.NoReferenceNumber = true;
                return View(default(PackagingCompliancePaymentDetailsViewModel));
            }

            var compliancePaymentResponse = await paymentFacadeService.GetCompliancePaymentDetailsForResubmissionAsync(
                new PackagingCompliancePaymentRequest
            {
                    ReferenceNumber = viewModel.ReferenceNumber,
                    MemberCount = viewModel.MemberCount,
                    Regulator = viewModel.NationCode,
                    ResubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime.Value) //payment facade in utc format                    
                });

            if (compliancePaymentResponse is null)
            {
                return View(default(PackagingCompliancePaymentDetailsViewModel));
            }

            var compliancePaymentDetailsViewModel = new PackagingCompliancePaymentDetailsViewModel
            {
                MemberCount = compliancePaymentResponse.MemberCount,
                PreviousPaymentReceived = ConvertToPoundsFromPence(compliancePaymentResponse.PreviousPaymentsReceived),
                ResubmissionFee = ConvertToPoundsFromPence(compliancePaymentResponse.ResubmissionFee),
                TotalOutstanding = ConvertToPoundsFromPence(compliancePaymentResponse.TotalOutstanding),
                ReferenceNumber = viewModel.ReferenceNumber
            };

            return View(compliancePaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
                $"Unable to retrieve the packaging compliance scheme payment details for " +
                $"{viewModel.SubmissionId} in {nameof(PackagingCompliancePaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            ViewBag.ReferenceNumber = viewModel.ReferenceNumber;

            return View(default(PackagingCompliancePaymentDetailsViewModel));
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}