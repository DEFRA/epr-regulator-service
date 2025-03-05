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
            // Mock data for Registration
            var registrationData = new List<ISubmissionDetailsViewModel>
            {
                new RegistrationDetailsViewModel
                {
                    OrganisationName = "Green Ltd",
                    SiteAddress = "23 Ruby St, London, E12 3SE",
                    OrganisationType = "Reprocessor",
                    Regulator = "Environment Agency (EA)"
                },
                new RegistrationDetailsViewModel
                {
                    OrganisationName = "Red Ltd",
                    SiteAddress = "89 Gold St, Birmingham, B12 4RT",
                    OrganisationType = "Producer",
                    Regulator = "Environment Agency (EA)"
                }
            };

            // Mock data for Accreditation
            var accreditationData = new List<ISubmissionDetailsViewModel>
            {
                new AccreditationDetailsViewModel
                {
                    OrganisationName = "Blue Ltd",
                    SiteAddress = "47 Sapphire Ave, Manchester, M15 4SN",
                    OrganisationType = "Exporter",
                    Regulator = "Scottish Environment Protection Agency",
                    AccreditationType = "Type A"
                },
                new AccreditationDetailsViewModel
                {
                    OrganisationName = "Yellow Ltd",
                    SiteAddress = "32 Diamond Rd, Leeds, LS8 3TG",
                    OrganisationType = "Recycler",
                    Regulator = "Natural Resources Wales",
                    AccreditationType = "Type B"
                }
            };

            // Combine both datasets
            var model = registrationData.Concat(accreditationData).ToList();

            return View(model);
        }
    }
}

