using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Web;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class LanguageSwitcherViewComponent : ViewComponent
{
    private readonly IOptions<RequestLocalizationOptions> _localizationOptions;
    private readonly IFeatureManager _featureManager;

    public LanguageSwitcherViewComponent(IOptions<RequestLocalizationOptions> localizationOptions, IFeatureManager featureManager)
    {
        _localizationOptions = localizationOptions;
        _featureManager = featureManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();

        var queryString = HttpUtility.ParseQueryString(Request.QueryString.ToString());

        string? token = queryString.Get("token");

        string? returnUrlInLink = queryString.Get("returnUrl");

        string returnPath = Request.Path.Value ?? "/";

        var returnUrl = token == null
            ? $"{returnPath}"
            : $"{returnPath}?token={token}";

        if (returnUrlInLink != null && token == null)
        {
            returnUrl += $"?returnUrl={returnUrlInLink}";
        }

        if (returnUrlInLink != null && token != null)
        {
            returnUrl += $"&returnUrl={returnUrlInLink}";
        }
        var languageSwitcherModel = new LanguageSwitcherModel
        {
            SupportedCultures = _localizationOptions.Value.SupportedCultures!.ToList(),
            CurrentCulture = cultureFeature!.RequestCulture.Culture,
            ReturnUrl = returnUrl,
            ShowLanguageSwitcher = await _featureManager.IsEnabledAsync(FeatureFlags.ShowLanguageSwitcher)
        };

        return View(languageSwitcherModel);
    }
}