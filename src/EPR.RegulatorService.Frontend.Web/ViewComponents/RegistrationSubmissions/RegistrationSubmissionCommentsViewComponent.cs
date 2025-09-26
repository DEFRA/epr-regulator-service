

using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.FronRegistrationSubmissionCommenttend.Web.ViewModels.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

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