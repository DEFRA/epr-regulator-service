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
    [Route(PagePath.UkSiteDetails)]
    public IActionResult UkSiteDetails()
    {
        CreateBackLinkToManageRegistrations();

        return View("~/Views/ReprocessorExporter/Registrations/UkSiteDetails.cshtml");
    }

    [HttpGet]
    [Route(PagePath.WasteLicences)]
    public IActionResult WasteLicences()
    {
        CreateBackLinkToManageRegistrations();

        return View("~/Views/ReprocessorExporter/Registrations/WasteLicences.cshtml");
    }

    [HttpGet]
    [Route(PagePath.InputsAndOutputs)]
    public IActionResult InputsAndOutputs()
    {
        CreateBackLinkToManageRegistrations();

        return View("~/Views/ReprocessorExporter/Registrations/InputsAndOutputs.cshtml");
    }

    private void CreateBackLinkToManageRegistrations()
    {
        ViewBag.BackLinkToDisplay = PagePath.ManageRegistrations;
        ViewBag.BackLinkAriaLabel = "Click here if you wish to go back to the previous page"; // TODO: Add to localizer
    }
}