using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

using Constants;

public class SubmissionsListViewComponent : ViewComponent
{
    private const string DirectProducer = "Direct Producer";
    private const string ComplianceScheme = "Compliance Scheme";
    private const string PendingStatus = "Pending";
    private const string AcceptedStatus = "Accepted";
    private const string RejectedStatus = "Rejected";
    private readonly IFacadeService _facadeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubmissionsListViewComponent(
        IFacadeService facadeService, IHttpContextAccessor httpContextAccessor)
    {
        _facadeService = facadeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ViewViewComponentResult> InvokeAsync(
        SubmissionListRequest request)
    {
        string organisationType = SetFilteredOrganisationType(
            request.IsDirectProducerChecked,
            request.IsComplianceSchemeChecked);

        string[] statuses = SetFilteredStatus(
            request.IsPendingSubmissionChecked,
            request.IsAcceptedSubmissionChecked,
            request.IsRejectedSubmissionChecked);

        var pagedOrganisationSubmissions
            = await _facadeService.GetOrganisationSubmissions(
                request.SearchOrganisationName,
                request.SearchOrganisationReference,
                organisationType,
                statuses,
                request.PageNumber);
        if ((request.PageNumber > pagedOrganisationSubmissions.TotalPages && request.PageNumber > 1) || request.PageNumber < 1)
        {
            _httpContextAccessor.HttpContext.Response.Redirect(PagePath.PrePageNotFound);
        }

        var model = new SubmissionsListViewModel
        {
            PagedOrganisationSubmissions = pagedOrganisationSubmissions.Items,
            PaginationNavigationModel = new PaginationNavigationModel
            {
                CurrentPage = pagedOrganisationSubmissions.CurrentPage,
                PageCount = pagedOrganisationSubmissions.TotalPages,
                ControllerName = "Submissions",
                ActionName = "Submissions"
            },
            RegulatorSubmissionFiltersModel = new RegulatorSubmissionFiltersModel
            {
                SearchOrganisationName = request.SearchOrganisationName,
                SearchOrganisationId = request.SearchOrganisationReference,
                IsDirectProducerChecked = request.IsDirectProducerChecked,
                IsComplianceSchemeChecked = request.IsComplianceSchemeChecked,
                IsPendingSubmissionChecked = request.IsPendingSubmissionChecked,
                IsAcceptedSubmissionChecked = request.IsAcceptedSubmissionChecked,
                IsRejectedSubmissionChecked = request.IsRejectedSubmissionChecked
            }
        };

        return View(model);
    }

    private static string SetFilteredOrganisationType(bool isDirectProducerSelected, bool isComplianceSchemeSelected)
    {
        if (isDirectProducerSelected && !isComplianceSchemeSelected)
        {
            return DirectProducer;
        }

        if (isComplianceSchemeSelected && !isDirectProducerSelected)
        {
            return ComplianceScheme;
        }

        return string.Empty;
    }

    private static string[] SetFilteredStatus(bool isPendingStatusSelected, bool isAcceptedStatusSelected, bool isRejectedStatusSelected)
    {
        List<string> submissionStatuses = new();

        if (isPendingStatusSelected)
        {
            submissionStatuses.Add(PendingStatus);
        }

        if (isAcceptedStatusSelected)
        {
            submissionStatuses.Add(AcceptedStatus);
        }

        if (isRejectedStatusSelected)
        {
            submissionStatuses.Add(RejectedStatus);
        }

        return submissionStatuses.ToArray();
    }
}