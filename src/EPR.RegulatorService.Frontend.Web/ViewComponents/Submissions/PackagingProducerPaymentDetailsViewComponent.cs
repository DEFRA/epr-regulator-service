using System.Threading.Tasks;

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
            var producerPaymentResponse = await paymentFacadeService
                .GetProducerPaymentDetailsForResubmissionAsync(new PackagingProducerPaymentRequest
            {
                // To Do:: Remove the hardcoded and uncomment the dynamic assignment once we have the confirmation about the input params
                ReferenceNumber = "dgregerg",
                Regulator = "GB-ENG",
                ResubmissionDate = new DateTime(2025, 01, 06, 11, 50, 47, 499, DateTimeKind.Utc)
                /*
                ReferenceNumber = viewModel.ReferenceNumber,
                Regulator = viewModel.NationCode,
                ResubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) //payment facade in utc format
                */
                });

            if (producerPaymentResponse is null)
            {
                return View(default(PackagingProducerPaymentDetailsViewModel));
            }

            var packagingProducerPaymentDetailsViewModel = new PackagingProducerPaymentDetailsViewModel
            {
                PreviousPaymentsReceived = ConvertToPoundsFromPence(producerPaymentResponse.PreviousPaymentsReceived),
                ResubmissionFee = ConvertToPoundsFromPence(producerPaymentResponse.ResubmissionFee),
                TotalOutstanding = ConvertToPoundsFromPence(producerPaymentResponse.TotalOutstanding),

                // TO Do:: Remove the hardcoded ReferenceNumber and uncomment the commenetd Referencenumber.
                // null check added for unit tests
                ReferenceNumber = viewModel.ReferenceNumber ?? "dgregerg",
                //ReferenceNumber = viewModel.ReferenceNumber,
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