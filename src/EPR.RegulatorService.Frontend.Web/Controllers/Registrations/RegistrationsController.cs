using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Registrations
{
    [FeatureGate(FeatureFlags.ManageRegistrations)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
    public class RegistrationsController : Controller
    {
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly string _pathBase;
        private readonly ExternalUrlsOptions _options;

        public RegistrationsController(ISessionManager<JourneySession> sessionManager,
            IConfiguration configuration, IOptions<ExternalUrlsOptions> options)
        {
            _sessionManager = sessionManager;
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
            _options = options.Value;
        }

        [HttpGet]
        [Route(PagePath.Registrations)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registrations(int? pageNumber, bool? clearFilters)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            session ??= new JourneySession();
            session.RegulatorRegistrationSession.RejectRegistrationJourneyData = null;
            session.RegulatorRegistrationSession.RegistrationFiltersModel ??= new RegistrationFiltersModel();

            clearFilters = clearFilters ?? false;
            if (clearFilters.Value)
            {
                ClearFilters(session);
            }

            SetCustomBackLink();

            var model = new RegistrationsViewModel
            {
                RegistrationFilters = new RegistrationFiltersModel()
                {
                    SearchOrganisationName = session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationName,
                    SearchOrganisationId = session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationId,
                    IsDirectProducerChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsDirectProducerChecked,
                    IsComplianceSchemeChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsComplianceSchemeChecked,
                    IsPendingRegistrationChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsPendingRegistrationChecked,
                    IsAcceptedRegistrationChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsAcceptedRegistrationChecked,
                    IsRejectedRegistrationChecked = session.RegulatorRegistrationSession.RegistrationFiltersModel.IsRejectedRegistrationChecked
                },
                PageNumber = pageNumber ?? 1,
                PowerBiLogin = _options.PowerBiLogin,
            };

            await SaveSessionAndJourney(session, PagePath.Registrations, PagePath.Registrations);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.Registrations)]
        public async Task<IActionResult> Registrations(
            RegistrationFiltersModel registrationFiltersModel)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            SetOrResetFilterValuesInSession(session, registrationFiltersModel);

            return await SaveSessionAndRedirect(
                session,
                nameof(Registrations),
                PagePath.Registrations,
                PagePath.Registrations,
                null);
        }

        private static void SetOrResetFilterValuesInSession(JourneySession session, RegistrationFiltersModel registrationFiltersModel)
        {
            var regulatorRegistrationSession = session.RegulatorRegistrationSession;
            if (registrationFiltersModel.ClearFilters)
            {
                ClearFilters(session);
            }
            else
            {
                if (IsFilterable(registrationFiltersModel))
                {
                    regulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationName = registrationFiltersModel.SearchOrganisationName;
                    regulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationId = registrationFiltersModel.SearchOrganisationId;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsDirectProducerChecked = registrationFiltersModel.IsDirectProducerChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsComplianceSchemeChecked = registrationFiltersModel.IsComplianceSchemeChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsPendingRegistrationChecked = registrationFiltersModel.IsPendingRegistrationChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsAcceptedRegistrationChecked = registrationFiltersModel.IsAcceptedRegistrationChecked;
                    regulatorRegistrationSession.RegistrationFiltersModel.IsRejectedRegistrationChecked = registrationFiltersModel.IsRejectedRegistrationChecked;
                    regulatorRegistrationSession.PageNumber = 1;
                }
            }
        }

        private static void ClearFilters(JourneySession session)
        {
            session.RegulatorRegistrationSession.PageNumber = 1;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationName = string.Empty;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.SearchOrganisationId = string.Empty;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsDirectProducerChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsComplianceSchemeChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsPendingRegistrationChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsAcceptedRegistrationChecked = false;
            session.RegulatorRegistrationSession.RegistrationFiltersModel.IsRejectedRegistrationChecked = false;
        }

        private static bool IsFilterable(RegistrationFiltersModel registrationFiltersModel) =>
        (
            (!string.IsNullOrEmpty(registrationFiltersModel.SearchOrganisationName) ||
             registrationFiltersModel.IsDirectProducerChecked ||
             registrationFiltersModel.IsComplianceSchemeChecked ||
             registrationFiltersModel.IsPendingRegistrationChecked ||
             registrationFiltersModel.IsAcceptedRegistrationChecked ||
             registrationFiltersModel.IsRejectedRegistrationChecked)
            || registrationFiltersModel.IsFilteredSearch);

        private async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath)
        {
            ClearRestOfJourney(session, currentPagePath);

            session.RegulatorRegistrationSession.Journey.AddIfNotExists(nextPagePath);

            await SaveSession(session);
        }

        private void ClearRestOfJourney(JourneySession session, string currentPagePath)
        {
            var index = session.RegulatorRegistrationSession.Journey.IndexOf(currentPagePath);

            // this also cover if current page not found (index = -1) then it clears all pages
            session.RegulatorRegistrationSession.Journey = session.RegulatorRegistrationSession.Journey.Take(index + 1).ToList();
        }

        private async Task SaveSession(JourneySession session) =>
            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        private void SetCustomBackLink()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(
            JourneySession session,
            string actionName,
            string currentPagePath,
            string? nextPagePath,
            object? routeValues)
        {
            await SaveSessionAndJourney(session, currentPagePath, nextPagePath);

            return RedirectToAction(actionName, routeValues);
        }
    }
}