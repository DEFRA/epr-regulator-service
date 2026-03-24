using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Cookies;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Cookies;

[AllowAnonymous]
public class CookiesController : Controller
{
    private readonly ICookieService _cookieService;
    private readonly AnalyticsOptions _googleAnalyticsOptions;

    public CookiesController(
        ICookieService cookieService,
        IOptions<AnalyticsOptions> googleAnalyticsOptions)
    {
        _cookieService = cookieService;
        _googleAnalyticsOptions = googleAnalyticsOptions.Value;
    }

    [HttpPost]
    [Route(PagePath.UpdateCookieAcceptance)]
    public LocalRedirectResult UpdateAcceptance(string returnUrl, string cookies)
    {
        _cookieService.SetCookieAcceptance(cookies == CookieAcceptance.Accept, Request.Cookies, Response.Cookies);
        TempData[CookieAcceptance.CookieAcknowledgement] = cookies;

        return LocalRedirect(returnUrl);
    }

    [HttpPost]
    [Route(PagePath.AcknowledgeCookieAcceptance)]
    public LocalRedirectResult AcknowledgeAcceptance(string returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.HomePath();
        }

        return LocalRedirect(returnUrl);
    }

    [Route(PagePath.Cookies)]
    public IActionResult Detail(string returnUrl)
    {
        var hasUserAcceptedCookies = _cookieService.HasUserAcceptedCookies(Request.Cookies);

        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.HomePath();
        }

        var returnPath = string.IsNullOrEmpty(Request.Path.Value) ? Url.HomePath() : Request.Path.Value;

        var cookieViewModel = new CookieDetailViewModel
        {
            SessionCookieName = EprCookieOptions.SessionCookieName,
            B2CCookieName = EprCookieOptions.B2CCookieName,
            CookiePolicyCookieName = EprCookieOptions.CookiePolicyCookieName,
            AntiForgeryCookieName = EprCookieOptions.AntiForgeryCookieName,
            GoogleAnalyticsDefaultCookieName = _googleAnalyticsOptions.DefaultCookieName,
            GoogleAnalyticsAdditionalCookieName = _googleAnalyticsOptions.AdditionalCookieName,
            AuthenticationCookieName = EprCookieOptions.AuthenticationCookieName,
            TsCookieName = EprCookieOptions.TsCookieName,
            TempDataCookieName = EprCookieOptions.TempDataCookie,
            CookiesAccepted = hasUserAcceptedCookies,
            ReturnUrl = $"~{returnPath}{Request.QueryString}",
            ShowAcknowledgement = TempData[CookieAcceptance.CookieAcknowledgement] != null,
        };
        
        ViewBag.BackLinkToDisplay = Url.Content(returnUrl);
        ViewBag.CurrentPage = returnUrl;
        
        return View(cookieViewModel);
    }
}
