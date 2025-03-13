using EPR;
using EPR.Common.Authorization.Constants;
using EPR.RegulatorService;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter
{
    [FeatureGate(FeatureFlags.ReprocessorExporter)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)] // PAUL -- need to create new policy going forward
    [Route(PagePath.ReprocessorExporterHome)]
    public class ApprovalHomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new ReprocessorExporterHomeViewModel
            {
                OrganisationName = "Green Ltd",
                SiteAddress = "23 Ruby St, London, E12 3SE",
                OrganisationType = "Exporter",
                Regulator = "Environment Agency (EA)",
                IsRegistration = true
            };

            return View("~/Views/ReprocessorExporter/ApprovalHome/Index.cshtml", model);
        }
    }
}

