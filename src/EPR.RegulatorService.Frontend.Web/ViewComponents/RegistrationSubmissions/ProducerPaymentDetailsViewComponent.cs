using System.Globalization;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;

public class ProducerPaymentDetailsViewComponent(IPaymentFacadeService paymentFacadeService, ILogger<ProducerPaymentDetailsViewComponent> logger) : ViewComponent
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
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                NoOfSubsidiariesOnlineMarketplace = 1, /* to do: will be in a new property*/
                NumberOfSubsidiaries = 1, /* to do: will be in a new property*/
                IsLateFeeApplicable = false, /* to do: will be in a new property*/
                IsProducerOnlineMarketplace = false, /* to do: will be in a new property*/
                ProducerType = "large", /* to do: will be in a new property*/
                Regulator = ((Core.Enums.CountryName)viewModel.NationId).GetDescription(), /*to do: get the nation code in a new property*/
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*to do: get from the RegistrationDateTime*/
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
                TotalChargeableItems = ConvertToPoundsFromPence(producerPaymentResponse.TotalChargeableItems),
                TotalOutstanding = ConvertToPoundsFromPence(producerPaymentResponse.TotalOutstanding),
                ProducerSize = "Large", // To do - check if this is the right property
                NumberOfSubsidiaries = 3, // To do - assign from request object
                NumberOfSubsidiariesBeingOnlineMarketplace = producerPaymentResponse.SubsidiariesFeeBreakdown.OnlineMarketPlaceSubsidiariesCount // To do - assign from request object
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