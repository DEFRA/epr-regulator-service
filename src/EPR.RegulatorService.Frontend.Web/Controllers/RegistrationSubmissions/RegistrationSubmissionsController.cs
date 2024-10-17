namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.Common.Authorization.Constants;
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Enums;

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
        [Route(PagePath.RegistrationSubmissionDetails + "/{organisationId:guid}")]
        public async Task<IActionResult> RegistrationSubmissionDetails(Guid organisationId)
        {
            SetBackLink(PagePath.RegistrationSubmissions);

            var model = new RegistrationSubmissionDetailsViewModel
            {
                OrganisationId = organisationId,
                OrganisationReference = "215 148",
                OrganisationName = "Acme org Ltd.",
                RegistrationReferenceNumber = "REF001",
                ApplicationReferenceNumber = "REF002",
                OrganisationType = RegistrationSubmissionOrganisationType.large,
                BusinessAddress = new BusinessAddress
                {
                    BuildingName = string.Empty,
                    BuildingNumber = "10",
                    Street = "High Street",
                    County = "Randomshire",
                    PostCode = "A12 3BC"
                },
                CompaniesHouseNumber = "0123456",
                RegisteredNation = "Scotland",
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
                Status = RegistrationSubmissionStatus.queried
            };

            return View(nameof(RegistrationSubmissionDetails), model);
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
