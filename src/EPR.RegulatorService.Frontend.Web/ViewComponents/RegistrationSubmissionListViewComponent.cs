

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;
 

public class RegistrationSubmissionListViewComponent : ViewComponent
{

    [ExcludeFromCodeCoverage]
    public async Task<ViewViewComponentResult> InvokeAsync( )
    {  
        return View();
    }
     
}