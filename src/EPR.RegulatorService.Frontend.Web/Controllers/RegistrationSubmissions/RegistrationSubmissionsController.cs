namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.Common.Authorization.Constants;
    using EPR.RegulatorService.Frontend.Core.Extensions;
    using EPR.RegulatorService.Frontend.Core.Services;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Sessions;
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
        private readonly ISessionManager<JourneySession> _sessionManager;

        public RegistrationSubmissionsController(
                        //ISessionManager<RegulatorSession> sessionManager,
                        //IFacadeService facadeService,
            IConfiguration configuration,
                        IOptions<ExternalUrlsOptions> externalUrlsOptions
                        )
        {
            _sessionManager = new JourneySessionManager();
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
            _externalUrlsOptions = externalUrlsOptions.Value;
        }

        [HttpGet]
        [Consumes("application/json")]
        [Route(PagePath.RegistrationSubmissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrationSubmissions(int? pageNumber)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
            session.RegulatorSession.CurrentPageNumber = pageNumber ?? session.RegulatorSession.CurrentPageNumber ?? 1;

            SetCustomBackLink();

            ViewBag.PowerBiLogin = _externalUrlsOptions.PowerBiLogin;

            // promote this model to store filters and the data and the page numbers
            var model = new RegistrationSubmissionsViewModel
            {
                PageNumber = pageNumber ?? 1,
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin,
                FilteredDataList = new RegistrationSubmissionsListViewModel()
            };

            await SaveSessionAndJourney(session, PagePath.Submissions, PagePath.Submissions);

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

        private async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath)
        {
            ClearRestOfJourney(session, currentPagePath);

            session.RegulatorRegistrationSession.Journey.AddIfNotExists(nextPagePath);

            await SaveSession(session);
        }

        private static void ClearRestOfJourney(JourneySession session, string currentPagePath)
        {
            int index = session.RegulatorRegistrationSession.Journey.IndexOf(currentPagePath);
            session.RegulatorRegistrationSession.Journey = session.RegulatorRegistrationSession.Journey.Take(index + 1).ToList();
        }

        private async Task SaveSession(JourneySession session) =>
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }
}
