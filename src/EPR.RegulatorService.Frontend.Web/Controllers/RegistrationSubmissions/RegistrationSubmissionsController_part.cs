namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
    using Microsoft.AspNetCore.Mvc;

    public partial class RegistrationSubmissionsController
    {
        public static void InitialiseOrContinuePaging(RegulatorRegistrationSubmissionSession session,
                                                int? pageNumber) => session.CurrentPageNumber = pageNumber ?? session.CurrentPageNumber ?? 1;

        private RegistrationSubmissionsViewModel InitialiseOrCreateViewModel(RegulatorRegistrationSubmissionSession session)
        {
            var existingSessionFilters = session.LatestFilterChoices ?? new RegistrationSubmissionsFilterViewModel()
            {
                PageNumber = 1
            };
            existingSessionFilters.Page = session.CurrentPageNumber;

            return new RegistrationSubmissionsViewModel
            {
                ListViewModel = new RegistrationSubmissionsListViewModel
                {
                    RegistrationsFilterModel = existingSessionFilters,
                    PaginationNavigationModel = new ViewModels.Shared.PaginationNavigationModel
                    {
                        CurrentPage = session.CurrentPageNumber.Value
                    }
                },
                PowerBiLogin = _externalUrlsOptions.PowerBiLogin
            };
        }

        private static void ClearFilters(RegulatorRegistrationSubmissionSession session,
                                  RegistrationSubmissionsFilterViewModel filters,
                                  bool performClearance)
        {
            if (!performClearance)
            {
                session.ClearFilters = false;
                if (null != filters)
                {
                    filters.ClearFilters = false;
                }
                return;
            }

            session.ClearFilters = true;
            if (null != filters)
            {
                filters.ClearFilters = true;
            }
        }

        private static void UpdateRegistrationSubmissionFiltersInSession(
                            RegulatorRegistrationSubmissionSession session,
                            RegistrationSubmissionsFilterViewModel filters,
                            bool performUpdate)
        {
            if (!performUpdate)
            {
                return;
            }

            session.LatestFilterChoices = filters;
        }

        private IActionResult? ReturnIfAppropriate(RegistrationSubmissionsFilterViewModel? filters, string? filterType) =>
                (filters, filterType) switch
                {
                    (null, null) => RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions"),
                    (null, FilterActions.SubmitFilters) => RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions"),
                    (_, not FilterActions.ClearFilters and not FilterActions.SubmitFilters) => RedirectToAction(PagePath.PageNotFound, "RegistrationSubmissions"),
                    _ => null
                };

        private void SetBacklinkToHome()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }
        private string GetCustomBackLink(string path)
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            return $"/{pathBase}/{path}";
        }

        private void SetBackLink(string path)
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.BackLinkToDisplay = $"/{pathBase}/{path}";
        }
    }
}
