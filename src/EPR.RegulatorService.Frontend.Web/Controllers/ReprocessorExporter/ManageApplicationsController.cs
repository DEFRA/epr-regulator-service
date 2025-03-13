using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ManageApprovals)]
public class ManageApplicationsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.BackLinkToDisplay = "";

        var model = new ManageApplicationsViewModel
        {
            ApplicationType = ApplicationType.Registration,
            ApplicationOrganisationType = ApplicationOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/ManageApplications/Index.cshtml", model);
    }
}