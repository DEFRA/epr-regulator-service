using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Home;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Home;

[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public class HomeController : RegulatorSessionBaseController
{
    private readonly ISessionManager<JourneySession> _sessionManager;
    private readonly IFeatureManager _featureManager;
    private readonly LandingPageConfig _landingPageConfig;
    private readonly EprCookieOptions _cookieOptions;

    public HomeController(
        ISessionManager<JourneySession> sessionManager,
        IFeatureManager featureManager,
        IOptions<LandingPageConfig> landingPageConfig,
        IOptions<EprCookieOptions> cookieOptions,
        IConfiguration configuration)
        : base(sessionManager, configuration)
    {
        _sessionManager = sessionManager;
        _featureManager = featureManager;
        _landingPageConfig = landingPageConfig.Value;
        _cookieOptions = cookieOptions.Value;
    }

    [AllowAnonymous]
    [Route(PagePath.SignedOut)]
    public IActionResult SignedOut()
    {
        HttpContext.Session.Clear();
        HttpContext.Response.Cookies.Delete(_cookieOptions.SessionCookieName);
        return View();
    }

    [HttpGet]
    [ActionName("LandingPage")]
    [Route(PagePath.Home)]
    public async Task<IActionResult> LandingPage()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        session ??= new JourneySession
        {
            UserData = HttpContext.User.GetUserData()
        };

        if (session.UserData.Organisations == null
            || session.UserData.Organisations.Count == 0)
        {
            throw new Exception("user orgs missing");
            return RedirectToAction(PagePath.Error, "Error");
        }

        string organisationName = session.UserData.Organisations[0].Name!;

        if (string.IsNullOrEmpty(session.UserData.FirstName) && string.IsNullOrEmpty(session.UserData.LastName))
        {
            var userData = HttpContext.User.GetUserData();
            session.UserData.FirstName = userData.FirstName;
            session.UserData.LastName = userData.LastName;
        }

        string personName = $"{session.UserData.FirstName} {session.UserData.LastName}";

        var viewModel = new LandingPageViewModel
        {
            OrganisationName = organisationName!,
            PersonName = personName,
            IsRegulatorAdmin = IsRegulatorAdmin(session.UserData),
            ManageAccountUrl = _landingPageConfig.ManageAccountUrl,
            ApplicationsUrl = _landingPageConfig.ApplicationsUrl,
            SubmissionsUrl = _landingPageConfig.SubmissionsUrl,
            RegistrationsUrl = _landingPageConfig.RegistrationsUrl,
            ManageApprovedPersonUrl = _landingPageConfig.ManageApprovedPersonUrl,
            ManageRegistrationSubmissionsUrl = await _featureManager.IsEnabledAsync(FeatureFlags.ManageRegistrationSubmissions)
            ? _landingPageConfig.ManageRegistrationSubmissionsUrl
            : string.Empty
        };

        await SaveSessionAndJourney(session, PagePath.Home);

        return View(nameof(LandingPage), viewModel);
    }

    private static bool IsRegulatorAdmin(UserData userData) =>
        userData.ServiceRoleId == (int)Core.Enums.ServiceRole.RegulatorAdmin;

    private async Task SaveSessionAndJourney(JourneySession session, string currentPagePath)
    {
        ClearJourneyAndSetCurrent(session, currentPagePath);

        await SaveSession(session);
    }

    private static void ClearJourneyAndSetCurrent(JourneySession session, string currentPagePath) =>
        session.RegulatorSession.Journey = new List<string> { currentPagePath };
}
