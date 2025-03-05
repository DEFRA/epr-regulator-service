namespace EPR.RegulatorService.Frontend.Web.Controllers.RegulatorExporter.Search
{
    using EPR.Common.Authorization.Constants;
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.FeatureManagement.Mvc;

    [FeatureGate(FeatureFlags.RegulatorExporter)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)] // PAUL -- need to create new policy going forward
    [Route(PagePath.RegulatorExporterSearch)]

    public class RegulatorExporterSearchController : Controller
    {
        public IActionResult Index()
        {
            // Mock data, later to be replaced by facade services
            var registrationData = new List<ISubmissionDetailsViewModel>
        {
            new RegistrationDetailsViewModel
            {
                OrganisationName = "Green Ltd",
                SiteAddress = "23 Ruby St, London, E12 3SE",
                OrganisationType = "Reprocessor",
                Regulator = "Environment Agency (EA)"
            }
        };

            var accreditationData = new List<ISubmissionDetailsViewModel>
        {
            new AccreditationDetailsViewModel
            {
                OrganisationName = "Blue Ltd",
                SiteAddress = "47 Sapphire Ave, Manchester, M15 4SN",
                OrganisationType = "Exporter",
                Regulator = "Scottish Environment Protection Agency",
                AccreditationType = "Type A"
            }
        };

            // Combine both datasets
            var model = registrationData.Concat(accreditationData).ToList();

            return View(model);
        }
    }
}