using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class CookieBannerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var consentCookie = Request.Cookies[EprCookieOptions.CookiePolicyCookieName];

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
