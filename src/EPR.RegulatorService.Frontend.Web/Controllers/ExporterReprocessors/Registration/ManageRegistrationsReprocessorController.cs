using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ExporterReprocessors.Registration;

[FeatureGate(FeatureFlags.ExporterReprocessors)]
[Route(PagePath.ManageReprocessorRegistration)]
public class ManageRegistrationsReprocessorController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.BackLinkToDisplay = "";

        return View();
    }
}