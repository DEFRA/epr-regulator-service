using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;

public class PackagingProducerPaymentDetailsViewComponent(IOptions<PaymentDetailsOptions> options, IPaymentFacadeService paymentFacadeService, ILogger<PackagingProducerPaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(PackagingProducerPaymentDetailsViewComponent)),
            "An error occurred while retrieving the packaging payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(SubmissionDetailsViewModel viewModel)
    {
        try
        {
            if (viewModel.ReferenceFieldNotAvailable) {
                this.ViewBag.NoReferenceField = this.ViewBag.NoReferenceNumber = true;
                return View(default(PackagingProducerPaymentDetailsViewModel));
            }

            if (viewModel.ReferenceNotAvailable)
            {
                this.ViewBag.NoReferenceNumber = true;
                return View(default(PackagingProducerPaymentDetailsViewModel));
            }

            var producerPaymentResponse = await paymentFacadeService
                .GetProducerPaymentDetailsForResubmissionAsync(new PackagingProducerPaymentRequest
            {
                ReferenceNumber = viewModel.ReferenceNumber,
                Regulator = viewModel.NationCode,
                ResubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.SubmittedDate) //payment facade in utc format
            });

            if (producerPaymentResponse is null)
            {
                return View(default(PackagingProducerPaymentDetailsViewModel));
            }

            var packagingProducerPaymentDetailsViewModel = new PackagingProducerPaymentDetailsViewModel
            {
                PreviousPaymentsReceived = ConvertToPoundsFromPence(producerPaymentResponse.PreviousPaymentsReceived),
                ResubmissionFee = ConvertToPoundsFromPence(producerPaymentResponse.ResubmissionFee),
                TotalOutstanding = ConvertToPoundsFromPence(PaymentHelper.GetUpdatedTotalOutstanding(producerPaymentResponse.TotalOutstanding, options.Value.ShowZeroFeeForTotalOutstanding)),
                ReferenceNumber = viewModel.ReferenceNumber,
                NationCode = viewModel.NationCode
            };

            return View(packagingProducerPaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
               $"Unable to retrieve the packaging producer payment details for {viewModel.SubmissionId} in {nameof(PackagingProducerPaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            ViewBag.ReferenceNumber = viewModel.ReferenceNumber;
            return View(default(PackagingProducerPaymentDetailsViewModel));
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}