using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ExporterReprocessors.Submissions
{
    [FeatureGate(FeatureFlags.ExporterReprocessors)]
    [Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)] // PAUL -- need to create new policy going forward
    [Route(PagePath.ExporterReprocessorSubmission)]
    public class ManageSubmissionController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}