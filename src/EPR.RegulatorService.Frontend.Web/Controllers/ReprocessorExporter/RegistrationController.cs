using System.Reflection;

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
    //public IActionResult MaterialsAuthorisedOnSite()
    //{
    //    // TODO: 515213
    //    return View("~/Views/ReprocessorExporter/Registration/MaterialsAuthorisedOnSite.cshtml", model);
    //}

    //public IActionResult SiteAddress()
    //{
    //    // TODO: 515162
    //    return View("~/Views/ReprocessorExporter/Registration/SiteAddress.cshtml", model);
    //}

    [Route(PagePath.WasteLicences)]
    public IActionResult WasteLicences()
    {
        ViewBag.BackLinkToDisplay = PagePath.WasteLicences;
        ViewBag.Title = "io hiop";

        // TODO: 515218
        return View("~/Views/ReprocessorExporter/Registration/WasteLicences.cshtml");
    }
}