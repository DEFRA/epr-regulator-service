using System.Linq;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.Submissions;

public class PackagingCompliancePaymentDetailsViewComponent(IPaymentFacadeService paymentFacadeService, ILogger<PackagingCompliancePaymentDetailsViewComponent> logger) : ViewComponent
{
    private static readonly Action<ILogger, string, Exception?> _logViewComponentError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(PackagingCompliancePaymentDetailsViewComponent)),
            "An error occurred while retrieving the packaging payment details: {ErrorMessage}");

    public async Task<ViewViewComponentResult> InvokeAsync(BaseSubmissionDetailsViewModel viewModel)
    {
        try
        {
            ////var compliancePaymentResponse = await paymentFacadeService.GetCompliancePaymentDetailsAsync(new CompliancePaymentRequest
            ////{
            ////    ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
            ////    Regulator = viewModel.NationCode,
            ////    ComplianceSchemeMembers = viewModel.CSOMembershipDetails?.Select(x => (ComplianceSchemeMemberRequest)x),
            ////    SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*payment facade in utc format*/
            ////});

            var csoMembershipDetailsDtos = new List<ComplianceSchemeMemberRequest> {
                new() {
                    IsLateFeeApplicable = true,
                    IsOnlineMarketplace = true,
                    MemberId = "109223",
                    MemberType = "large",
                    NoOfSubsidiariesOnlineMarketplace = 23,
                    NumberOfSubsidiaries = 68
                },
                new() {
                    IsLateFeeApplicable = true,
                    IsOnlineMarketplace = true,
                    MemberId = "109222",
                    MemberType = "large",
                    NoOfSubsidiariesOnlineMarketplace = 60,
                    NumberOfSubsidiaries = 105
                }
            };

            var compliancePaymentResponse = await paymentFacadeService.GetCompliancePaymentDetailsAsync(new CompliancePaymentRequest
            {
                ApplicationReferenceNumber = "PEPR10380624P1",
                Regulator = "GB-ENG",
                ComplianceSchemeMembers = csoMembershipDetailsDtos,
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2024, 12, 20, 15, 01, 37, DateTimeKind.Utc)) /*payment facade in utc format*/
            });

            if (compliancePaymentResponse is null)
            {
                return View(default(PackagingCompliancePaymentDetailsViewModel));
            }

            viewModel.CSOMembershipDetails = [
                new() {
                    IsLateFeeApplicable = true,
                    IsOnlineMarketPlace = true,
                    MemberId = "109223",
                    MemberType = "large",
                    NoOfSubsidiariesOnlineMarketplace = 23,
                    NumberOfSubsidiaries = 68
                },
                new() {
                    IsLateFeeApplicable = true,
                    IsOnlineMarketPlace = true,
                    MemberId = "109222",
                    MemberType = "large",
                    NoOfSubsidiariesOnlineMarketplace = 60,
                    NumberOfSubsidiaries = 105
                }
            ];

            var compliancePaymentDetailsViewModel = new PackagingCompliancePaymentDetailsViewModel
            {
                ResubmissionFee = 0.00M,
                SubTotal = ConvertToPoundsFromPence(compliancePaymentResponse.TotalChargeableItems),
                PreviousPaymentReceived = ConvertToPoundsFromPence(compliancePaymentResponse.PreviousPaymentsReceived),
                TotalOutstanding = ConvertToPoundsFromPence(compliancePaymentResponse.TotalOutstanding),
            };

            return View(compliancePaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
                $"Unable to retrieve the packaging compliance scheme payment details for " +
                $"{viewModel.SubmissionId} in {nameof(PackagingCompliancePaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            return View(default(PackagingCompliancePaymentDetailsViewModel));
        }
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}