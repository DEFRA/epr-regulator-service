
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

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
        ViewBag.BackLinkToDisplay = PagePath.ManageRegistrations;

        var model = new ManageRegistrationsViewModel
        {
            ApplicationOrganisationType = ApplicationOrganisationType.Exporter
        };

        return View("~/Views/ReprocessorExporter/Registrations/WasteLicences.cshtml", model);
    }
}