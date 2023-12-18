using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions
{

    public class SubmissionsListViewModel
    {
        public IEnumerable<Submission>? PagedOrganisationSubmissions { get; set; }

        public PaginationNavigationModel PaginationNavigationModel { get; set; }
        
        public RegulatorSubmissionFiltersModel RegulatorSubmissionFiltersModel { get; set; }
    }
}