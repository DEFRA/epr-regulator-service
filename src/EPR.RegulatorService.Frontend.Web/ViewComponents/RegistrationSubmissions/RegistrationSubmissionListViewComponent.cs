

using System.Diagnostics.CodeAnalysis;

using EPR;
using EPR.RegulatorService;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;


public class RegistrationSubmissionListViewComponent : ViewComponent
{

    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync()
    {
        return View();
    }

}