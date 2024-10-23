

using System.Diagnostics.CodeAnalysis;

using EPR;
using EPR.RegulatorService;
using EPR.RegulatorService.Frontend;
using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.ViewComponents; 

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions;


public class RegistrationSubmissionCommentsComponent : ViewComponent
{
    public string Comments { get; set; }
    public string Title { get; set; }

    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync(string comments, string title)
    {
        Comments = comments;
        Title = title;

        return View();
    }

}