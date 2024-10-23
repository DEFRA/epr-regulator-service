

using System.Diagnostics.CodeAnalysis;

using EPR;
using EPR.RegulatorService;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;


public class RegistrationSubmissionCommentsViewComponent : ViewComponent
{  
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync( string comments, string title)
    {
        var model = new RegistrationSubmissionComment { Title = title, Comment = comments };

        return View(model);
    } 
} 