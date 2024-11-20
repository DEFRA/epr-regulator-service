using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Extensions;
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
                Regulator = ((Core.Enums.CountryName)viewModel.NationId).GetDescription(), /*get the nation code in a new property*/
                ComplianceSchemeMembers =
                [
                    new ComplianceSchemeMemberRequest { MemberId = "addhere", MemberType = "addhere" }
                ],
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*payment facade in utc format*/
            });

            if (compliancePaymentResponse is null)
            {
                return View(default(CompliancePaymentDetailsViewModel));
            }

            // To do:: map the domain to view model as part of https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_workitems/edit/477227
            return View(new CompliancePaymentDetailsViewModel());
        }
        catch (Exception ex)
        {
            _logViewComponentError.Invoke(logger,
                $"Unable to retrieve the compliance scheme payment details for " +
                $"{viewModel.SubmissionId} in {nameof(CompliancePaymentDetailsViewComponent)}.{nameof(InvokeAsync)}", ex);

            return View(default(CompliancePaymentDetailsViewModel));
        }
    }
}