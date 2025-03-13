using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route(PagePath.ManageApprovals)]
public class ManageApprovalsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.BackLinkToDisplay = "";

        var model = new ManageApprovalsViewModel
        {
            ApprovalType = ApprovalType.Registration,
            ApprovalOrganisationType = ApprovalOrganisationType.Reprocessor
        };

        return View("~/Views/ReprocessorExporter/ManageApprovals/Index.cshtml", model);
    }
}