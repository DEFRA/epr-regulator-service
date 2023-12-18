using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class CookieBannerViewComponent : ViewComponent
{
    private readonly EprCookieOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CookieBannerViewComponent"/> class.
    /// </summary>
    /// <param name="options"></param>
    public CookieBannerViewComponent(IOptions<EprCookieOptions> options)
    {
        _options = options.Value;
    }

    public IViewComponentResult Invoke()
    {
        var consentCookie = Request.Cookies[_options.CookiePolicyCookieName];

        var cookieAcknowledgement = TempData[CookieAcceptance.CookieAcknowledgement]?.ToString();

        var dontShowBanner = ViewContext.RouteData.Values["controller"]?.ToString() == "Cookies";

        var cookieBannerModel = new CookieBannerModel()
        {
            CurrentPage = Request.Path,
            ShowBanner = !dontShowBanner && cookieAcknowledgement == null && consentCookie == null,
            ShowAcknowledgement = !dontShowBanner && cookieAcknowledgement != null,
            AcceptAnalytics = cookieAcknowledgement == CookieAcceptance.Accept,
            ReturnUrl = $"{Request.PathBase}{Request.Path}{Request.QueryString}",
        };

        return View(cookieBannerModel);
    }
}
