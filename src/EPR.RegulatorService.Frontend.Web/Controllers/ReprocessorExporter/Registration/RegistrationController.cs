namespace EPR.RegulatorService.Frontend.Web.Controllers.ReExJourney.Registration
{
    using EPR.RegulatorService.Frontend.Web.Controllers.ReExJourney.Common;

    using Microsoft.AspNetCore.Mvc;

    public class RegistrationController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
