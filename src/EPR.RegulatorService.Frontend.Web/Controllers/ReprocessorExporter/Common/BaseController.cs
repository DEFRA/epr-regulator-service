namespace EPR.RegulatorService.Frontend.Web.Controllers.ReExJourney.Common
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
