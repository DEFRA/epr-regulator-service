using EPR;
using EPR.RegulatorService;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.Registration)]
public class RegistrationController : Controller
{
    [HttpGet]
    [Route(PagePath.AuthorisedMaterials)]
    public IActionResult AuthorisedMaterials()
    {
        ViewBag.BackLinkToDisplay = PagePath.ManageApplications;

        var model = new ManageApplicationsViewModel
        {
            ApplicationType = ApplicationType.Registration,
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Reprocessor/AuthorisedMaterials.cshtml", model);

    }
    [HttpGet]
    [Route(PagePath.UkSiteDetails)]
    public IActionResult UkSiteDetails()
    {
        ViewBag.BackLinkToDisplay = PagePath.ManageApplications;

        var model = new ManageApplicationsViewModel
        {
            ApplicationType = ApplicationType.Registration,
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/Reprocessor/UkSiteDetails.cshtml", model);

    }

}