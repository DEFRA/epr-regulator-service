using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationController : Controller
{
    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public IActionResult UkSiteDetails()
    {
        ViewBag.BackLinkToDisplay = PagePath.ManageRegistrations;

        ViewBag.BackLinkAriaLabel = "Click here if you wish to go back to the previous page";//will be added to localizer

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Reprocessor/UkSiteDetails.cshtml", model);
    }
}
