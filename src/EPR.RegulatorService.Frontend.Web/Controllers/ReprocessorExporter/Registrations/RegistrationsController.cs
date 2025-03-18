using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController : Controller
{
    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public IActionResult UkSiteDetails()
    {
        ViewBag.BackLinkToDisplay = PagePath.ManageRegistrations;

        ViewBag.BackLinkAriaLabel = "Click here if you wish to go back to the previous page";//will be added to localizer

        return View("~/Views/ReprocessorExporter/Registrations/UkSiteDetails.cshtml");
    }
}
