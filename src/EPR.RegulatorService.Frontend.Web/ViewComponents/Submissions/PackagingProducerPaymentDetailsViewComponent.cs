using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;

public class PackagingProducerPaymentDetailsViewComponent(
    IOptions<PaymentDetailsOptions> options,
    IPaymentFacadeService paymentFacadeService,
    IFeatureManager featureManager,
    ILogger<PackagingProducerPaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(PackagingProducerPaymentDetailsViewComponent)),
            "An error occurred while retrieving the packaging payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(SubmissionDetailsViewModel viewModel)
    {
        if (viewModel.ReferenceFieldNotAvailable)
        {
            this.ViewBag.NoReferenceField = this.ViewBag.NoReferenceNumber = true;
            return View(default(PackagingProducerPaymentDetailsViewModel));
        }

        if (viewModel.ReferenceNotAvailable)
        {
            this.ViewBag.NoReferenceNumber = true;
            return View(default(PackagingProducerPaymentDetailsViewModel));
        }

        int memberCount =
            await featureManager.IsEnabledAsync(FeatureFlags.IncludeSubsidiariesInFeeCalculationsForRegulators)
                ? viewModel.MemberCount
                : 1;

        var producerPaymentResponse = await paymentFacadeService
            .GetProducerPaymentDetailsForResubmissionAsync(new PackagingProducerPaymentRequest
            {
                ReferenceNumber = viewModel.ReferenceNumber,
                Regulator = viewModel.NationCode,
                MemberCount = memberCount,
                ResubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.SubmittedDate) //payment facade in utc format
            });

        if (producerPaymentResponse is null)
        {
            return View(default(PackagingProducerPaymentDetailsViewModel));
        }

        var packagingProducerPaymentDetailsViewModel = new PackagingProducerPaymentDetailsViewModel
        {
            SubmissionHash = viewModel.SubmissionHash,
            PreviousPaymentsReceived = ConvertToPoundsFromPence(producerPaymentResponse.PreviousPaymentsReceived),
            ResubmissionFee = ConvertToPoundsFromPence(producerPaymentResponse.ResubmissionFee),
            TotalOutstanding = ConvertToPoundsFromPence(PaymentHelper.GetUpdatedTotalOutstanding(producerPaymentResponse.TotalOutstanding, options.Value.ShowZeroFeeForTotalOutstanding)),
            ReferenceNumber = viewModel.ReferenceNumber,
            NationCode = viewModel.NationCode,
            MemberCount = viewModel.MemberCount
        };

        return View(packagingProducerPaymentDetailsViewModel);
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}