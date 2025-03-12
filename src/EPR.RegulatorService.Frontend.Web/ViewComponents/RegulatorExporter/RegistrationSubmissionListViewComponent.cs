namespace EPR.RegulatorService.Frontend.Web.ViewComponents.ReprocessorExporter
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Common.Interfaces;

    using Microsoft.AspNetCore.Mvc;

    public class RepExpSubmissionListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ISubmissionDetailsViewModel> model)
        {
            return View("Default", model);
        }
    }
}
