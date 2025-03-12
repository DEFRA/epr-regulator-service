namespace EPR.RegulatorService.Frontend.Web.Controllers.ReExJourney.Accreditation
{
    using EPR.RegulatorService.Frontend.Web.Controllers.ReExJourney.Common;

    using Microsoft.AspNetCore.Mvc;

    public class AccreditationController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
