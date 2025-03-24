using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;

public class CompliancePaymentDetailsViewComponent(IPaymentFacadeService paymentFacadeService, ILogger<CompliancePaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(CompliancePaymentDetailsViewComponent)),
            "An error occurred while retrieving the payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionDetailsViewModel viewModel)
    {
        try
        {
            var compliancePaymentResponse = await paymentFacadeService.GetCompliancePaymentDetailsAsync(new CompliancePaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ReferenceNumber,
                Regulator = viewModel.NationCode,
                ComplianceSchemeMembers = viewModel.CSOMembershipDetails?.Select(x => (ComplianceSchemeMemberRequest)x),
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.IsResubmission
                ? viewModel.SubmissionDetails.TimeAndDateOfResubmission.Value
                : viewModel.SubmissionDetails.TimeAndDateOfSubmission)
            });

            if (compliancePaymentResponse is null)
            {
                return View(default(CompliancePaymentDetailsViewModel));
            }

            (var largeProducers, var smallProducers) = compliancePaymentResponse.ComplianceSchemeMembers
                .GetIndividualProducers(viewModel.CSOMembershipDetails);
            var lateProducers = compliancePaymentResponse.ComplianceSchemeMembers.GetLateProducers();
            var onlineMarketPlaces = compliancePaymentResponse.ComplianceSchemeMembers.GetOnlineMarketPlaces();
            var subsidiariesCompanies = compliancePaymentResponse.ComplianceSchemeMembers.GetSubsidiariesCompanies();

            var compliancePaymentDetailsViewModel = new CompliancePaymentDetailsViewModel
            {
                ApplicationFee = ConvertToPoundsFromPence(compliancePaymentResponse.ApplicationProcessingFee),
                SubTotal = ConvertToPoundsFromPence(compliancePaymentResponse.TotalChargeableItems),
                PreviousPaymentReceived = ConvertToPoundsFromPence(compliancePaymentResponse.PreviousPaymentsReceived),
                TotalOutstanding = ConvertToPoundsFromPence(compliancePaymentResponse.TotalOutstanding),
                SchemeMemberCount = compliancePaymentResponse.ComplianceSchemeMembers.Count,
                LargeProducerCount = largeProducers.Count,
                LargeProducerFee = ConvertToPoundsFromPence(largeProducers.GetFees()),
                SmallProducerCount = smallProducers.Count,
                SmallProducerFee = ConvertToPoundsFromPence(smallProducers.GetFees()),
                LateProducerCount = lateProducers.Count,
                LateProducerFee = ConvertToPoundsFromPence(lateProducers.Sum()),
                OnlineMarketPlaceCount = onlineMarketPlaces.Count,
                OnlineMarketPlaceFee = ConvertToPoundsFromPence(onlineMarketPlaces.Sum()),
                SubsidiariesCompanyCount = viewModel.CSOMembershipDetails.Sum(r => r.NumberOfSubsidiaries),
                SubsidiariesCompanyFee = ConvertToPoundsFromPence(subsidiariesCompanies.Sum())
            };

            return View(compliancePaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
                $"Unable to retrieve the compliance scheme payment details for " +
                $"{viewModel.SubmissionId} in {nameof(CompliancePaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            return View(default(CompliancePaymentDetailsViewModel));
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}