namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.Common.Authorization.Constants;
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.FeatureManagement.Mvc;

    [FeatureGate(FeatureFlags.ManageRegistrationSubmissions)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
    public class RegistrationSubmissionsController : Controller
    {
        private readonly string _pathBase;
        private readonly ExternalUrlsOptions _externalUrlsOptions;

        public RegistrationSubmissionsController(
            IConfiguration configuration,
            IOptions<ExternalUrlsOptions> externalUrlsOptions)
        {
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
            _externalUrlsOptions = externalUrlsOptions.Value;
        }

        [HttpGet]
        [Route(PagePath.RegistrationSubmissions)]
        public IActionResult RegistrationSubmissions()
        {
            SetCustomBackLink();

            var model = new RegistrationSubmissionsViewModel
            {
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
                FilteredDataList = new RegistrationSubmissionsListViewModel()
            };

            return View(model);
        }

        [HttpGet]
        [Route(PagePath.QueryRegistrationSubmission)]
        public async Task<IActionResult> QueryRegistrationSubmission()
        {
            SetBackLink(PagePath.RegistrationSubmissions);

            var model = new QueryRegistrationSubmissionViewModel();

            return View(nameof(QueryRegistrationSubmission), model);
        }

        [HttpPost]
        [Route(PagePath.QueryRegistrationSubmission)]
        public async Task<IActionResult> QueryRegistrationSubmission(QueryRegistrationSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetBackLink(PagePath.RegistrationSubmissions);
                return View(nameof(QueryRegistrationSubmission), model);
            }

            return Redirect(PagePath.RegistrationSubmissions);
        }

        [HttpGet]
        [Route(PagePath.RejectRegistrationSubmission)]
        public async Task<IActionResult> RejectRegistrationSubmission()
        {
            SetBackLink(PagePath.RegistrationSubmissions);

            var model = new RejectRegistrationSubmissionViewModel();

            return View(nameof(RejectRegistrationSubmission), model);
        }

        [HttpPost]
        [Route(PagePath.RejectRegistrationSubmission)]
        public async Task<IActionResult> RejectRegistrationSubmission(RejectRegistrationSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetBackLink(PagePath.RegistrationSubmissions);
                return View(nameof(RejectRegistrationSubmission), model);
            }

            return Redirect(PagePath.RegistrationSubmissions);
        }

        [HttpGet]
        [Route(PagePath.OrganisationRegistrationDetails)]
        public async Task<IActionResult> OrganisationRegistrationDetails()
        {
            SetBackLink(PagePath.RegistrationSubmissions);

            var model = new OrganisationRegistrationDetailsViewModel
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = "Shehzad Ltd.",
                RegistrationReferenceNumber = "REF001",
                OrganisationType = "Large Producer",
                BusinessAddress = new Core.Models.BusinessAddress
                {
                    BuildingName = string.Empty,
                    BuildingNumber = "10",
                    Street = "High Street",
                    County = "Randomshire",
                    PostCode = "A12 3BC"
                },
                CompaniesHouseNumber = string.Empty,
                RegisteredNation = "Scotland",
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin
            };

            return View(nameof(OrganisationRegistrationDetails), model);
        }

        private void SetCustomBackLink()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }

        private void SetBackLink(string path)
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.BackLinkToDisplay = $"/{pathBase}/{path}";
        }
    }
}
