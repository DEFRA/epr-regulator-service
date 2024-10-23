namespace EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
    using Microsoft.AspNetCore.Mvc;

    public partial class RegistrationSubmissionsController
    {
        public void InitialiseOrContinuePaging(RegulatorRegistrationSubmissionSession session,
                                                int? pageNumber) => session.CurrentPageNumber = pageNumber ?? session.CurrentPageNumber ?? 1;

        private RegistrationSubmissionsViewModel InitialiseOrCreateViewModel(RegulatorRegistrationSubmissionSession session)
        {
            var existingSessionFilters = session.LatestFilterChoices ?? new RegistrationSubmissionsFilterViewModel();

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

        private void ClearFilters(RegulatorRegistrationSubmissionSession session,
                                  RegistrationSubmissionsFilterViewModel filters,
                                  bool performClearance)
        {
            if (!performClearance)
            {
                return;
            }

            session.ClearFilters = filters.ClearFilters = true;
            session.LatestFilterChoices = filters;
        }

        private void UpdateRegistrationSubmissionFiltersInSession(
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
                    (null, null) => RedirectToPage(PagePath.PageNotFoundPath),
                    (null, FilterActions.SubmitFilters) => RedirectToPage(PagePath.PageNotFoundPath),
                    (_, not FilterActions.ClearFilters and not FilterActions.SubmitFilters) => RedirectToPage(PagePath.PageNotFoundPath),
                    _ => null
                };

        private void SetBacklinkToHome()
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.CustomBackLinkToDisplay = $"/{pathBase}/{PagePath.Home}";
        }

        private void SetBackLink(string path)
        {
            string pathBase = _pathBase.TrimStart('/').TrimEnd('/');
            ViewBag.BackLinkToDisplay = $"/{pathBase}/{path}";
        }
    }
}
