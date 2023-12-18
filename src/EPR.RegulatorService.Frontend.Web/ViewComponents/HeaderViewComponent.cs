namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

using System.Security.Claims;

using Configs;

using Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

public class HeaderViewComponent : ViewComponent
{
    private readonly ExternalUrlsOptions _options;

    public HeaderViewComponent(IOptions<ExternalUrlsOptions> options)
    {
        _options = options.Value;
    }

    public IViewComponentResult Invoke()
    {
        ViewBag.HeaderViewOverrideUrl = _options.LandingPageUrl;

        if (User is ClaimsPrincipal claimsPrincipal)
        {
            var userData = claimsPrincipal.TryGetUserData();
            if (userData is not null &&
                (string.IsNullOrEmpty(userData.EnrolmentStatus) &&
                !string.IsNullOrEmpty(ViewBag.CurrentPage)))
            {
                ViewBag.HeaderViewOverrideUrl = ViewBag.CurrentPage;
            }
        }

        return View("Default", _options);
    }
}