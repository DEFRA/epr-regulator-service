namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    public class RegistrationSubmissionsController : Controller
    {
        private readonly string _pathBase;
        private readonly ExternalUrlsOptions _externalUrlsOptions;

        public RegistrationSubmissionsController(
            IConfiguration configuration,
            IOptions<ExternalUrlsOptions> externalUrlsOptions)
        {
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
            _externalUrlsOptions = externalUrlsOptions.Value;
        }

        [HttpGet]
        [Route(PagePath.RegistrationSubmissions)]
        public IActionResult RegistrationSubmissions()
        {
            SetCustomBackLink();

            var model = new RegistrationSubmissionsViewModel
            {
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin
            };

            return View(model);
        }

        private void SetCustomBackLink()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }
    }
}
