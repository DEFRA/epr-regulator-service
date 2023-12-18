using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

public class PhaseBannerViewComponent : ViewComponent
{
    private readonly EPR.RegulatorService.Frontend.Web.Configs.PhaseBannerOptions _bannerOptions;

    public PhaseBannerViewComponent(IOptions<EPR.RegulatorService.Frontend.Web.Configs.PhaseBannerOptions> bannerOptions)
    {
        _bannerOptions = bannerOptions.Value;
    }

    public ViewViewComponentResult Invoke()
    {
        const string phaseBanner = "PhaseBanner";

        var phaseBannerModel = new EPR.RegulatorService.Frontend.Web.ViewModels.Shared.PhaseBannerModel()
        {
            Status = $"{phaseBanner}.{_bannerOptions!.ApplicationStatus}",
            Url = _bannerOptions!.SurveyUrl,
            ShowBanner = _bannerOptions!.Enabled
        };
        return View(phaseBannerModel);
    }
}