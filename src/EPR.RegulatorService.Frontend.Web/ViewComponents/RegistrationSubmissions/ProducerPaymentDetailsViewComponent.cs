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
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionDetailsViewModel viewModel)
    {
        try
        {
            var optionalModel = await paymentFacadeService.GetProducerPaymentDetailsAsync(new ProducerPaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                NoOfSubsidiariesOnlineMarketplace = 1, /* to do: will be in a new property*/
                NumberOfSubsidiaries = 1, /* to do: will be in a new property*/
                IsLateFeeApplicable = false, /* to do: will be in a new property*/
                IsProducerOnlineMarketplace = false, /* to do: will be in a new property*/
                ProducerType = "large", /* to do: will be in a new property*/
                Regulator = ((Core.Enums.CountryName)viewModel.NationId).GetDescription(), /*get the nation code in a new property*/
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*payment facade in utc format*/
            });

            if (!optionalModel.HasValue)
            {
                return View();
            }

            var model = optionalModel.Value;
            var producerPaymentDetailsViewModel = new ProducerPaymentDetailsViewModel
            {
                ApplicationProcessingFee = ConvertToPoundsFromPence(model.ApplicationProcessingFee),
                LateRegistrationFee = ConvertToPoundsFromPence(model.LateRegistrationFee),
                OnlineMarketplaceFee = ConvertToPoundsFromPence(model.OnlineMarketplaceFee),
                PreviousPaymentsReceived = ConvertToPoundsFromPence(model.PreviousPaymentsReceived),
                SubsidiaryFee = ConvertToPoundsFromPence(model.SubsidiaryFee),
                TotalChargeableItems = ConvertToPoundsFromPence(model.TotalChargeableItems),
                TotalOutstanding = ConvertToPoundsFromPence(model.TotalOutstanding)
            };

            return View(producerPaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Unable to retrieve the producer payment details for {SubmissionId}", viewModel.SubmissionId);
            return View();
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}