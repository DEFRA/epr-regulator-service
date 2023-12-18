namespace EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Core.Models;
using Core.Models.Submissions;

public class SubmissionsViewModel
{
    public int? PageNumber { get; set; }
    public string PowerBiLogin { get; set; }
    public EndpointResponseStatus? RejectSubmissionResult { get; set; } = EndpointResponseStatus.NotSet;
    public EndpointResponseStatus? AcceptSubmissionResult { get; set; } = EndpointResponseStatus.NotSet;
    public string? OrganisationName { get; set; }
    public SubmissionFiltersModel SubmissionFilters { get; set; }
}