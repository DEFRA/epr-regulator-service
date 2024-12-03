using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
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
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                Regulator = viewModel.NationCode,
                ComplianceSchemeMembers = viewModel.CSOMembershipDetails?.Select(x => (ComplianceSchemeMemberRequest)x),
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*payment facade in utc format*/
            });

            if (compliancePaymentResponse is null)
            {
                return View(default(CompliancePaymentDetailsViewModel));
            }

            #region "No Member Type from payment facade"
            ////IList<ComplianceSchemeMember> largeProducers1 = [];
            ////IList<ComplianceSchemeMember> smallProducers1 = [];
            ////foreach (var csoMembershipDetail in viewModel.CSOMembershipDetails)
            ////{
            ////    //Filter the member based on the member id match between the req and res object
            ////    var complianceSchemeMember = compliancePaymentResponse.ComplianceSchemeMembers
            ////        .Find(r => r.MemberId == csoMembershipDetail.MemberId);

            ////    //Check the member type from the request object to filter the large producers
            ////    if (csoMembershipDetail.MemberType.Equals("Large", StringComparison.OrdinalIgnoreCase))
            ////    {
            ////        largeProducers1.Add(complianceSchemeMember);
            ////        break;
            ////    }

            ////    //Check the member type from the request object to filter the small producers
            ////    if (csoMembershipDetail.MemberType.Equals("Small", StringComparison.OrdinalIgnoreCase))
            ////    {
            ////        smallProducers1.Add(complianceSchemeMember);
            ////        break;
            ////    }
            ////}

            ////int lpCount1 = largeProducers1.Count;
            ////decimal lpFee1 = largeProducers1.Sum(r => r.MemberFee);

            ////int spCount1 = smallProducers1.Count;
            ////decimal spFee1 = smallProducers1.Sum(r => r.MemberFee);
            #endregion

            #region "Member Type from payment facade"
            var largeProducers2 = compliancePaymentResponse.ComplianceSchemeMembers.Where(
                r => r.MemberType.Equals("Large", StringComparison.OrdinalIgnoreCase)).ToList();

            var smallProducers2 = compliancePaymentResponse.ComplianceSchemeMembers.Where(
                r => r.MemberType.Equals("Small", StringComparison.OrdinalIgnoreCase)).ToList();

            int lpCount2 = largeProducers2.Count;
            decimal lpFee2 = largeProducers2.Sum(r => r.MemberFee);

            int spCount2 = smallProducers2.Count;
            decimal spFee2 = smallProducers2.Sum(r => r.MemberFee);
            #endregion

            var onlineMarketPlaces = compliancePaymentResponse.ComplianceSchemeMembers.Select(r => r.OnlineMarketPlaceFee);
            var subsidiariesCompanies = compliancePaymentResponse.ComplianceSchemeMembers.Select(r => r.SubsidiaryFee);
            var lateProducers = compliancePaymentResponse.ComplianceSchemeMembers.Select(r => r.LateRegistrationFee);

            return View(new CompliancePaymentDetailsViewModel
            {
                ApplicationFee = ConvertToPoundsFromPence(compliancePaymentResponse.ApplicationProcessingFee),
                SubTotal = ConvertToPoundsFromPence(compliancePaymentResponse.TotalChargeableItems),
                PreviousPaymentReceived = ConvertToPoundsFromPence(compliancePaymentResponse.PreviousPaymentsReceived),
                TotalOutstanding = ConvertToPoundsFromPence(compliancePaymentResponse.TotalOutstanding),
                SchemeMemberCount = compliancePaymentResponse.ComplianceSchemeMembers.Count,
                LargeProducerCount = lpCount2,
                LargeProducerFee = ConvertToPoundsFromPence(lpFee2),
                SmallProducerCount = spCount2,
                SmallProducerFee = ConvertToPoundsFromPence(spFee2),
                LateProducerCount = lateProducers.Count(),
                LateProducerFee = ConvertToPoundsFromPence(lateProducers.Sum()),
                OnlineMarketPlaceCount = onlineMarketPlaces.Count(),
                OnlineMarketPlaceFee = ConvertToPoundsFromPence(onlineMarketPlaces.Sum()),
                SubsidiariesCompanyCount = subsidiariesCompanies.Count(),
                SubsidiariesCompanyFee = ConvertToPoundsFromPence(subsidiariesCompanies.Sum())
            });
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