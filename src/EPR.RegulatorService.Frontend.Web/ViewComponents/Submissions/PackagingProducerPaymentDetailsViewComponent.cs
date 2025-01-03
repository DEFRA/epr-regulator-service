using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;

public class PackagingProducerPaymentDetailsViewComponent(IPaymentFacadeService paymentFacadeService, ILogger<PackagingProducerPaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(PackagingProducerPaymentDetailsViewComponent)),
            "An error occurred while retrieving the packaging payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(BaseSubmissionDetailsViewModel viewModel)
    {
        try
        {
            var producerPaymentResponse = await paymentFacadeService.GetProducerPaymentDetailsAsync<PackagingProducerPaymentResponse>(new ProducerPaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                NoOfSubsidiariesOnlineMarketplace = viewModel.ProducerDetails.NoOfSubsidiariesOnlineMarketPlace,
                NumberOfSubsidiaries = viewModel.ProducerDetails.NoOfSubsidiaries,
                IsLateFeeApplicable = viewModel.ProducerDetails.IsLateFeeApplicable,
                IsProducerOnlineMarketplace = viewModel.ProducerDetails.IsProducerOnlineMarketplace,
                ProducerType = viewModel.ProducerDetails.ProducerType,
                Regulator = viewModel.NationCode,
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime)
            });

            if (producerPaymentResponse is null)
            {
                return View(default(PackagingProducerPaymentDetailsViewModel));
            }

            var packagingProducerPaymentDetailsViewModel = new PackagingProducerPaymentDetailsViewModel
            {
                ResubmissionFee = ConvertToPoundsFromPence(producerPaymentResponse.ResubmissionFee),
                PreviousPaymentsReceived = ConvertToPoundsFromPence(producerPaymentResponse.PreviousPaymentsReceived),
                SubTotal = ConvertToPoundsFromPence(producerPaymentResponse.TotalChargeableItems),
                TotalOutstanding = ConvertToPoundsFromPence(producerPaymentResponse.TotalOutstanding),
            };

            return View(packagingProducerPaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
               $"Unable to retrieve the packaging producer payment details for {viewModel.SubmissionId} in {nameof(PackagingProducerPaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            return View(default(PackagingProducerPaymentDetailsViewModel));
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}