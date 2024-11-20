using System.Diagnostics.CodeAnalysis;
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
    public async Task<ViewViewComponentResult> InvokeAsync(RegistrationSubmissionDetailsViewModel viewModel)
    {
        try
        {
            var model = await paymentFacadeService.GetCompliancePaymentDetailsAsync(new CompliancePaymentRequest
            {
                ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
                Regulator = ((Core.Enums.CountryName)viewModel.NationId).GetDescription(), /*get the nation code in a new property*/
                ComplianceSchemeMembers =
                [
                    new ComplianceSchemeMember { MemberId = "addhere", MemberType = "addhere" }
                ],
                SubmissionDate = TimeZoneInfo.ConvertTimeToUtc(viewModel.RegistrationDateTime) /*payment facade in utc format*/
            });

            // To do:: map the domain to view model
            return View(new CompliancePaymentDetailsViewModel());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to retrieve the compliance scheme payment details for {SubmissionId}", viewModel.SubmissionId);
            return View();
        }
    }
}