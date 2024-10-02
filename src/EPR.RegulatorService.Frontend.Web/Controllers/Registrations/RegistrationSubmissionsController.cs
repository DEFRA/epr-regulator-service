namespace EPR.RegulatorService.Frontend.Web.Controllers.Registrations
{
    using EPR.RegulatorService.Frontend.Web.Constants;

    using Microsoft.AspNetCore.Mvc;

    public class RegistrationSubmissionsController : Controller
    {
        private readonly string _pathBase;

        public RegistrationSubmissionsController(
            IConfiguration configuration)
        {
            _pathBase = configuration.GetValue<string>(ConfigKeys.PathBase);
        }

        [HttpGet]
        [Route(PagePath.RegistrationSubmissions)]
        public IActionResult RegistrationSubmissions()
        {
            SetCustomBackLink();

            return View();
        }

        private void SetCustomBackLink()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }
    }
}
