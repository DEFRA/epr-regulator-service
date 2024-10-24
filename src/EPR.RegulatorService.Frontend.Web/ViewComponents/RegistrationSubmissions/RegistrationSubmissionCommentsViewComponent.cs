

using System.Diagnostics.CodeAnalysis;

using EPR;
using EPR.RegulatorService;
using EPR.RegulatorService.FronRegistrationSubmissionCommenttend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Localization;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;


public class RegistrationSubmissionCommentsViewComponent : ViewComponent
{  
    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync( string comment, string title)
    {
        var model = new RegistrationSubmissionCommentViewModel { Title = title, Comment = comment };

        return View(model);
    } 
} 