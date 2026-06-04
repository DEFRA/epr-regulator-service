using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;

public class CompliancePaymentDetailsViewComponent(
    IOptions<PaymentDetailsOptions> options,
    IPaymentFacadeService paymentFacadeService,
    ILogger<CompliancePaymentDetailsViewComponent> logger) : ViewComponent
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
            var csoMembers = viewModel.CSOMembershipDetails ?? [];

            var compliancePaymentResponse = await paymentFacadeService.GetCompliancePaymentDetailsAsync(
                new CompliancePaymentRequest
                {
                    ApplicationReferenceNumber = viewModel.ReferenceNumber,
                    Regulator = viewModel.NationCode,
                    ComplianceSchemeMembers = csoMembers.Select(m => MapToComplianceSchemeMemberRequest(m, viewModel)),
                    SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.IsResubmission
                        ? viewModel.SubmissionDetails.TimeAndDateOfResubmission.GetValueOrDefault()
                        : viewModel.SubmissionDetails.TimeAndDateOfSubmission),
                    IncludeRegistrationFee = viewModel.RegistrationJourneyType != RegistrationJourneyType.CsoSmallProducer
                });

            if (compliancePaymentResponse is null)
            {
                return View(default(CompliancePaymentDetailsViewModel));
            }

            var (largeProducers, smallProducers) = compliancePaymentResponse.ComplianceSchemeMembers
                .GetIndividualProducers(csoMembers);
            var lateProducers = compliancePaymentResponse.ComplianceSchemeMembers.GetLateProducers();
            var onlineMarketPlaces = compliancePaymentResponse.ComplianceSchemeMembers.GetOnlineMarketPlaces();
            var complianceSchemeMembers = compliancePaymentResponse.ComplianceSchemeMembers;
            var closedLoopRecyclingFees = complianceSchemeMembers.GetClosedLoopRecyclingFees();

            var compliancePaymentDetailsViewModel = new CompliancePaymentDetailsViewModel
            {
                ApplicationFee = ConvertToPoundsFromPence(compliancePaymentResponse.ApplicationProcessingFee),
                SubTotal = ConvertToPoundsFromPence(compliancePaymentResponse.TotalChargeableItems),
                PreviousPaymentReceived =
                    ConvertToPoundsFromPence(compliancePaymentResponse.PreviousPaymentsReceived),
                TotalOutstanding =
                    ConvertToPoundsFromPence(PaymentHelper.GetUpdatedTotalOutstanding(
                        compliancePaymentResponse.TotalOutstanding, options.Value.ShowZeroFeeForTotalOutstanding)),
                SchemeMemberCount = compliancePaymentResponse.ComplianceSchemeMembers.Count,
                LargeProducerCount = largeProducers.Count,
                LargeProducerFee = ConvertToPoundsFromPence(largeProducers.GetFees()),
                SmallProducerCount = smallProducers.Count,
                SmallProducerFee = ConvertToPoundsFromPence(smallProducers.GetFees()),
                LateProducerCount = lateProducers.Count,
                LateProducerFee = ConvertToPoundsFromPence(lateProducers.Sum()),
                OnlineMarketPlaceCount = onlineMarketPlaces.Count,
                OnlineMarketPlaceFee = ConvertToPoundsFromPence(onlineMarketPlaces.Sum()),
                ClosedLoopRegistrationCount = closedLoopRecyclingFees.Count,
                ClosedLoopRecyclingFee = ConvertToPoundsFromPence(closedLoopRecyclingFees.Sum()),
                SubsidiariesCompanyCount = csoMembers.Sum(r => r.NumberOfSubsidiaries),
                SubsidiariesCompanyFee = ConvertToPoundsFromPence(complianceSchemeMembers.GetSubsidiariesCompanyNetFees()),
                SubsidiariesClosedLoopRecyclingCount = complianceSchemeMembers.GetSubsidiariesClosedLoopRecyclingCount(),
                SubsidiariesClosedLoopRecyclingFee =
                    ConvertToPoundsFromPence(complianceSchemeMembers.GetSubsidiariesClosedLoopRecyclingFees()),
                ResubmissionStatus = viewModel.ResubmissionStatus,
                Status = viewModel.Status
            };

            return View(compliancePaymentDetailsViewModel);
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
                $"Unable to retrieve the compliance scheme payment details for " +
                $"{viewModel.SubmissionId} in {nameof(CompliancePaymentDetailsViewComponent)}.{nameof(InvokeAsync)}",
                ex);

            return View(default(CompliancePaymentDetailsViewModel));
        }
    }

    private static ComplianceSchemeMemberRequest MapToComplianceSchemeMemberRequest(
        CsoMembershipDetailsDto csoMember,
        RegistrationSubmissionDetailsViewModel viewModel)
    {
        var isCsoLargeProducer = viewModel.RegistrationJourneyType == RegistrationJourneyType.CsoLargeProducer;

        int noOfHoldingCompaniesClosedLoopRecycling;
        int noOfSubsidiariesClosedLoopRecycling;
        if (!isCsoLargeProducer)
        {
            noOfHoldingCompaniesClosedLoopRecycling = 0;
            noOfSubsidiariesClosedLoopRecycling = 0;
        }
        else
        {
            noOfHoldingCompaniesClosedLoopRecycling = csoMember.NumberOfHoldingCompaniesClosedLoopRecycling ?? 0;
            noOfSubsidiariesClosedLoopRecycling = csoMember.NumberOfSubsidiariesClosedLoopRecycling;
        }

        return new ComplianceSchemeMemberRequest
        {
            MemberId = csoMember.MemberId,
            MemberType = csoMember.MemberType,
            IsOnlineMarketplace = csoMember.IsOnlineMarketPlace,
            IsLateFeeApplicable = csoMember.IsLateFeeApplicable,
            NoOfHoldingCompaniesClosedLoopRecycling = noOfHoldingCompaniesClosedLoopRecycling,
            IsClosedLoopRecycling = noOfHoldingCompaniesClosedLoopRecycling > 0,
            NoOfSubsidiariesClosedLoopRecycling = noOfSubsidiariesClosedLoopRecycling,
            NumberOfSubsidiaries = csoMember.NumberOfSubsidiaries,
            NoOfSubsidiariesOnlineMarketplace = csoMember.NumberOfSubsidiariesOnlineMarketPlace
        };
    }

    private static decimal ConvertToPoundsFromPence(decimal amount) => amount / 100;
}
