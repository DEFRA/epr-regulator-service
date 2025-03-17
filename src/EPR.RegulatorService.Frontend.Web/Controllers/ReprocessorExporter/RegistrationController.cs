
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
public class RegistrationController : Controller
{
    [Route(PagePath.WasteLicences)]
    public IActionResult WasteLicences()
    {
        // tbd
        ViewBag.BackLinkToDisplay = PagePath.WasteLicences;

        return View("~/Views/ReprocessorExporter/Registration/WasteLicences.cshtml");
    }
}