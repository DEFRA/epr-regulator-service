using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;

public class ProducerPaymentDetailsViewComponent(IOptions<PaymentDetailsOptions> options,
    IPaymentFacadeService paymentFacadeService, ILogger<ProducerPaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(ProducerPaymentDetailsViewComponent)),
            "An error occurred while retrieving the payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionDetailsViewModel viewModel)
    {
        try
        {
            var producerPaymentResponse = await paymentFacadeService.GetProducerPaymentDetailsAsync(new ProducerPaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ReferenceNumber,
                NoOfSubsidiariesOnlineMarketplace = viewModel.ProducerDetails.NoOfSubsidiariesOnlineMarketPlace,
                NumberOfSubsidiaries = viewModel.ProducerDetails.NoOfSubsidiaries,
                NoOfLateSubsidiaries = viewModel.ProducerDetails.NoOfLateSubsidiaries,
                IsLateFeeApplicable = viewModel.ProducerDetails.IsLateFeeApplicable,
                IsProducerOnlineMarketplace = viewModel.ProducerDetails.IsProducerOnlineMarketplace,
                ProducerType = viewModel.ProducerDetails.ProducerType,
                Regulator = viewModel.NationCode,
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.IsResubmission
                ? viewModel.SubmissionDetails.TimeAndDateOfResubmission.Value
                : viewModel.SubmissionDetails.TimeAndDateOfSubmission)
            });

            if (producerPaymentResponse is null)
            {
                return View(default(ProducerPaymentDetailsViewModel));
            }

            var producerPaymentDetailsViewModel = new ProducerPaymentDetailsViewModel
            {
                ApplicationProcessingFee = ConvertToPoundsFromPence(producerPaymentResponse.ApplicationProcessingFee),
                LateRegistrationFee = ConvertToPoundsFromPence(producerPaymentResponse.LateRegistrationFee),
                OnlineMarketplaceFee = ConvertToPoundsFromPence(producerPaymentResponse.OnlineMarketplaceFee),
                PreviousPaymentsReceived = ConvertToPoundsFromPence(producerPaymentResponse.PreviousPaymentsReceived),
                SubsidiaryFee = ConvertToPoundsFromPence(producerPaymentResponse.SubsidiaryFee - producerPaymentResponse.SubsidiariesFeeBreakdown.SubsidiaryOnlineMarketPlaceFee),
                SubsidiaryOnlineMarketPlaceFee = ConvertToPoundsFromPence(producerPaymentResponse.SubsidiariesFeeBreakdown.SubsidiaryOnlineMarketPlaceFee),
                SubTotal = ConvertToPoundsFromPence(producerPaymentResponse.TotalChargeableItems),
                TotalOutstanding = ConvertToPoundsFromPence(PaymentHelper.GetUpdatedTotalOutstanding(producerPaymentResponse.TotalOutstanding, options.Value.ShowZeroFeeForTotalOutstanding)),
                ProducerSize = $"{char.ToUpperInvariant(viewModel.ProducerDetails.ProducerType[0])}{viewModel.ProducerDetails.ProducerType[1..]}",
                NumberOfSubsidiaries = viewModel.ProducerDetails.NoOfSubsidiaries,
                NumberOfSubsidiariesBeingOnlineMarketplace = producerPaymentResponse.SubsidiariesFeeBreakdown.OnlineMarketPlaceSubsidiariesCount,
                ResubmissionStatus = viewModel.ResubmissionStatus,
                Status = viewModel.Status,
            };

            return View(producerPaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
               $"Unable to retrieve the producer payment details for {viewModel.SubmissionId} in {nameof(ProducerPaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            return View(default(ProducerPaymentDetailsViewModel));
        }
    }
    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}