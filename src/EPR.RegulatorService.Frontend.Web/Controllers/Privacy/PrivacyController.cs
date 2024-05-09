using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Privacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Privacy;

[AllowAnonymous]
public class PrivacyController : Controller
{
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly EmailAddressOptions _emailOptions;
    private readonly SiteDateOptions _siteDateOptions;

    public PrivacyController(
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<EmailAddressOptions> emailOptions,
        IOptions<SiteDateOptions> siteDateOptions)
    {
        _urlOptions = urlOptions.Value;
        _emailOptions = emailOptions.Value;
        _siteDateOptions = siteDateOptions.Value;
    }

    [HttpGet]
    [Route(PagePath.Privacy)]
    public IActionResult Detail(string returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.HomePath();
        }

        var model = new PrivacyViewModel
        {
            DataProtectionPublicRegisterUrl = _urlOptions.PrivacyDataProtectionPublicRegister,
            DefrasPersonalInformationCharterUrl = _urlOptions.PrivacyDefrasPersonalInformationCharter,
            InformationCommissionerUrl = _urlOptions.PrivacyInformationCommissioner,
            ScottishEnvironmentalProtectionAgencyUrl = _urlOptions.PrivacyScottishEnvironmentalProtectionAgency,
            NationalResourcesWalesUrl = _urlOptions.PrivacyNationalResourcesWales,
            NorthernIrelandEnvironmentAgencyUrl = _urlOptions.PrivacyNorthernIrelandEnvironmentAgency,
            EnvironmentAgencyUrl = _urlOptions.PrivacyEnvironmentAgency,
            DataProtectionEmail = _emailOptions.DataProtection,
            DefraGroupProtectionOfficerEmail = _emailOptions.DefraGroupProtectionOfficer,
            LastUpdated = _siteDateOptions.PrivacyLastUpdated.ToString(_siteDateOptions.DateFormat, System.Globalization.CultureInfo.InvariantCulture)
        };
        ViewBag.BackLinkToDisplay = returnUrl;
        ViewBag.CurrentPage = returnUrl;

        return View(model);
    }
}
