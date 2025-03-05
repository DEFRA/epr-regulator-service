using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.RegulatorExporter.Home
{
    [FeatureGate(FeatureFlags.RegulatorExporter)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)] // PAUL -- need to create new policy going forward
    [Route(PagePath.RegulatorExporterHome)]
    public class RegulatorExporterHomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new RegulatorExporterHomeViewModel
            {
                OrganisationName = "Green Ltd",
                SiteAddress = "23 Ruby St, London, E12 3SE",
                OrganisationType = "Reprocessor",
                Regulator = "Environment Agency (EA)"
            };

            return View(model);
        }
    }
}

