using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
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
            var producerPaymentResponse = await paymentFacadeService.GetProducerPaymentDetailsAsync(new ProducerPaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber ?? "PEPR10577624P1",
                NoOfSubsidiariesOnlineMarketplace = viewModel.ProducerDetails?.NoOfSubsidiariesOnlineMarketPlace ?? 0,
                NumberOfSubsidiaries = viewModel.ProducerDetails?.NoOfSubsidiaries ?? 0,
                IsLateFeeApplicable = viewModel.ProducerDetails?.IsLateFeeApplicable ?? false,
                IsProducerOnlineMarketplace = viewModel.ProducerDetails?.IsProducerOnlineMarketplace ?? false,
                ProducerType = viewModel.ProducerDetails?.ProducerType ?? "Large",
                Regulator = viewModel.NationCode ?? "GB-ENG",
                SubmissionDate = new DateTime(2024, 12, 20, 18, 56, 00, DateTimeKind.Utc) //TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime)
            });

            if (producerPaymentResponse is null)
            {
                return View(default(PackagingProducerPaymentDetailsViewModel));
            }

            var packagingProducerPaymentDetailsViewModel = new PackagingProducerPaymentDetailsViewModel
            {
                ResubmissionFee = 0.00M,
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