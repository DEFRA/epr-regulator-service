using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ReprocessorExporterRegistrations)]
public class RegistrationsController : Controller
{
    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public IActionResult InputsAndOutputs()
    {
        ViewBag.BackLinkToDisplay = PagePath.ManageRegistrations;

        return View("~/Views/ReprocessorExporter/Registrations/InputsAndOutputs.cshtml");
    }
}