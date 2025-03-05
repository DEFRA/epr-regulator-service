namespace EPR.RegulatorService.Frontend.Web.ViewComponents.RegulatorExporter
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorExporter.Common.Interfaces;

    using Microsoft.AspNetCore.Mvc;

    public class RegExpSubmissionListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ISubmissionDetailsViewModel> model)
        {
            return View("Default", model);
        }
    }
}
