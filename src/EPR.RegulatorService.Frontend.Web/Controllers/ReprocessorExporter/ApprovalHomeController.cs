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
    // Define multiple routes for different combinations
    [Route(PagePath.ReprocessorExporterHome)] // manage-registrations-reprocessor
    [Route(PagePath.ReprocessorExporterAccreditation)] // manage-accreditation-reprocessor
    [Route(PagePath.ExporterRegistrationHome)] // manage-registrations-exporter
    [Route(PagePath.ExporterAccreditationHome)] // manage-accreditation-exporter
    public class ApprovalHomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Determine organisation type and registration/accreditation dynamically
            var isRegistration = Request.Path.Value.Contains("registrations");
            var isExporter = Request.Path.Value.Contains("exporter");

            var model = new ReprocessorExporterHomeViewModel
            {
                OrganisationName = "Green Ltd",
                SiteAddress = "23 Ruby St, London, E12 3SE",
                OrganisationType = isExporter ? "Exporter" : "Reprocessor",
                Regulator = "Environment Agency (EA)",
                IsRegistration = isRegistration
            };

            return View("~/Views/ReprocessorExporter/ApprovalHome/Index.cshtml", model);
        }
    }
}

