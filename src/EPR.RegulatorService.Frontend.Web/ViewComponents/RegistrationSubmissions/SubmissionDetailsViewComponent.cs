namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewComponents;

    using System.Threading.Tasks;

    public class SubmissionDetailsViewComponent : ViewComponent
    {
        public async Task<ViewViewComponentResult> InvokeAsync(SubmissionDetailsViewModel model)
        {
            return View(model);
        }
    }
}
