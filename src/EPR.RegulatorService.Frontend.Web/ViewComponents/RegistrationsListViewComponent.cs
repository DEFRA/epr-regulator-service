using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using EPR.RegulatorService.Frontend.Web.Controllers.Registrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

using Core.Models.Registrations;

public class RegistrationsListViewComponent : ViewComponent
{
    private const string PendingStatus = "Pending";
    private const string AcceptedStatus = "Accepted";
    private const string RejectedStatus = "Rejected";
    private readonly IFacadeService _facadeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegistrationsListViewComponent(
        IFacadeService facadeService,
        IHttpContextAccessor httpContextAccessor)
    {
        _facadeService = facadeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ViewViewComponentResult> InvokeAsync(
        RegistrationListRequest request)
    {
        var organisationType = SetFilteredOrganisationType(
            request.IsDirectProducerChecked,
            request.IsComplianceSchemeChecked);

        string[] statuses = SetFilteredStatus(
            request.IsPendingRegistrationChecked,
            request.IsAcceptedRegistrationChecked,
            request.IsRejectedRegistrationChecked);

        var pagedOrganisationRegistrations
            = await _facadeService.GetOrganisationSubmissions<Registration>(
                request.SearchOrganisationName,
                request.SearchOrganisationReference,
                organisationType,
                statuses,
                request.PageNumber);

        if ((request.PageNumber > pagedOrganisationRegistrations.TotalPages && request.PageNumber > 1)|| request.PageNumber < 1)
        {
            _httpContextAccessor.HttpContext.Response.Redirect(PagePath.PageNotFoundPath);
        }

        foreach(Registration item in pagedOrganisationRegistrations.Items)
       {
           item.OrganisationDetailsFileId = item.CompanyDetailsFileId;
           item.OrganisationDetailsFileName = item.CompanyDetailsFileName;
            item.RejectionComments = item.Comments;
       }
      
       var model = new RegistrationsListViewModel()
        {
            PagedOrganisationRegistrations = pagedOrganisationRegistrations.Items,
            PaginationNavigationModel = new PaginationNavigationModel
            {
                CurrentPage = pagedOrganisationRegistrations.CurrentPage,
                PageCount = pagedOrganisationRegistrations.TotalPages,
                ControllerName = "Registrations",
                ActionName = nameof(RegistrationsController.Registrations)
            },
            RegulatorRegistrationFiltersModel = new RegulatorRegistrationFiltersModel
            {
                SearchOrganisationName = request.SearchOrganisationName,
                SearchOrganisationId = request.SearchOrganisationReference,
                IsDirectProducerChecked = request.IsDirectProducerChecked,
                IsComplianceSchemeChecked = request.IsComplianceSchemeChecked,
                IsPendingRegistrationChecked = request.IsPendingRegistrationChecked,
                IsAcceptedRegistrationChecked = request.IsAcceptedRegistrationChecked,
                IsRejectedRegistrationChecked = request.IsRejectedRegistrationChecked,
            }
        };

        return View(model);
    }

    private static OrganisationType? SetFilteredOrganisationType(bool isDirectProducerSelected, bool isComplianceSchemeSelected)
    {
        if (isDirectProducerSelected && !isComplianceSchemeSelected)
        {
            return OrganisationType.DirectProducer;
        }

        if (isComplianceSchemeSelected && !isDirectProducerSelected)
        {
            return OrganisationType.ComplianceScheme;
        }

        return null;
    }

    private static string[] SetFilteredStatus(bool isPendingStatusSelected, bool isAcceptedStatusSelected, bool isRejectedStatusSelected)
    {
        List<string> registrationStatuses = new();

        if (isPendingStatusSelected)
        {
            registrationStatuses.Add(PendingStatus);
        }

        if (isAcceptedStatusSelected)
        {
            registrationStatuses.Add(AcceptedStatus);
        }

        if (isRejectedStatusSelected)
        {
            registrationStatuses.Add(RejectedStatus);
        }

        return registrationStatuses.ToArray();
    }
}