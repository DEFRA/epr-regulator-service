using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ApprovedPersonListPage;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorSearchPage;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

using ServiceRole = EPR.RegulatorService.Frontend.Core.Enums.ServiceRole;

namespace EPR.RegulatorService.Frontend.Web.Controllers.SearchManageApprovers;

[FeatureGate(FeatureFlags.ManageApprovedUsers)]
[Authorize(Policy = PolicyConstants.RegulatorBasicPolicy)]
public class SearchManageApproversController : RegulatorSessionBaseController
{
    private readonly IFacadeService _facadeService;

    public SearchManageApproversController(
        ISessionManager<JourneySession> sessionManager,
        ILogger<SearchManageApproversController> logger,
        IConfiguration configuration,
        IFacadeService facadeService)
        : base(sessionManager, logger, configuration)
    {
        _facadeService = facadeService;
    }

    [HttpGet]
    [Route(PagePath.RegulatorSearchPage)]
    public async Task<ActionResult> RegulatorSearchPage()
    {
        ViewBag.BackLinkToDisplay = GetHomeBackLink();
        var model = new SearchTermViewModel();
        return View(model);
    }

    [HttpPost]
    [Route(PagePath.RegulatorSearchPage)]
    public async Task<ActionResult> RegulatorSearchPage(SearchTermViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        session.SearchManageApproversSession = new SearchManageApproversSession
        {
            SearchTerm = model.SearchTerm
        };

        if (ModelState.IsValid)
        {
            return await SaveSessionAndRedirect(session,
                nameof(RegulatorSearchResult),
                PagePath.RegulatorSearchPage,
                PagePath.RegulatorSearchResult, null);
        }

        ViewBag.BackLinkToDisplay = GetHomeBackLink();
        return View(model);
    }

    [HttpGet]
    [Route(PagePath.RegulatorSearchResult)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RegulatorSearchResult(int? pageNumber)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        var searchManageApproversSession = session.SearchManageApproversSession;

        searchManageApproversSession.CurrentPageNumber = pageNumber ?? searchManageApproversSession.CurrentPageNumber ?? 1;

        var organisationSearchResults = await _facadeService
            .GetOrganisationBySearchTerm(searchManageApproversSession.SearchTerm, searchManageApproversSession.CurrentPageNumber.Value);

        var model = new OrganisationSearchResultsListViewModel
        {
            PagedOrganisationSearchResults = organisationSearchResults.Items,
            PaginationNavigationModel = new PaginationNavigationModel
            {
                CurrentPage = organisationSearchResults.CurrentPage,
                PageCount = organisationSearchResults.TotalPages,
                ControllerName = "SearchManageApprovers",
                ActionName = nameof(RegulatorSearchResult)
            },
            OrganisationSearchFilterModel = new OrganisationSearchFilterModel
            {
                SearchTerm = searchManageApproversSession.SearchTerm
            }
        };

        await AddCurrentPageAndSaveSession(session, PagePath.RegulatorSearchResult);
        SetBackLink(session, PagePath.RegulatorSearchResult);
        return View(model);
    }

    [HttpGet]
    [Route(PagePath.RegulatorCompanyDetail)]
    public async Task<IActionResult> RegulatorCompanyDetail(Guid organisationId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (organisationId != Guid.Empty)
        {
            session.RegulatorSession.OrganisationId = organisationId;
        }
        var organisationDetails = await _facadeService.GetRegulatorCompanyDetails(session.RegulatorSession.OrganisationId.Value);

        var model = GetCompanyDetailsRequest(organisationDetails, session.RegulatorSession.OrganisationId.Value);

        await AddCurrentPageAndSaveSession(session, PagePath.RegulatorCompanyDetail);
        SetBackLink(session, PagePath.RegulatorCompanyDetail);
        return View(model);
    }

    [HttpGet]
    [Route(PagePath.ApprovedPersonListPage)]
    public async Task<ActionResult> ApprovedPersonListPage(Guid externalOrganisationId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
        if (externalOrganisationId != Guid.Empty)
        {
            session.RegulatorSession.OrganisationId = externalOrganisationId;
        }
        var results = await _facadeService.GetProducerOrganisationUsersByOrganisationExternalId(session.RegulatorSession.OrganisationId.Value);

        var model = new OrganisationUsersModel
        {
            ExternalOrganisationId = session.RegulatorSession.OrganisationId.Value,
            OrganisationUsers = results.Select(user => new OrganisationUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PersonExternalId = user.PersonExternalId
            }).ToList()
        };

        await AddCurrentPageAndSaveSession(session, PagePath.ApprovedPersonListPage);
        SetBackLink(session, PagePath.ApprovedPersonListPage);

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.ApprovedPersonListPage)]
    public async Task<ActionResult> ApprovedPersonListPage(OrganisationUsersModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        if (ModelState.IsValid)
        {
            // TODO
            // Whoever is implementing the next page will have to edit this redirect
            // For now, let's just go back to the search results page

            return await SaveSessionAndRedirect(session,
                nameof(RegulatorSearchResult),
                PagePath.RegulatorSearchPage,
                PagePath.RegulatorSearchResult, null);
        }

        var results = await _facadeService.GetProducerOrganisationUsersByOrganisationExternalId(model.ExternalOrganisationId);
        model.OrganisationUsers = results.Select(user => new OrganisationUser
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PersonExternalId = user.PersonExternalId
        }).ToList();

        await AddCurrentPageAndSaveSession(session, PagePath.ApprovedPersonListPage);
        SetBackLink(session, PagePath.ApprovedPersonListPage);

        return View(model);
    }
    private static RegulatorCompanyDetailViewModel GetCompanyDetailsRequest(RegulatorCompanyDetailsModel organisationDetails, Guid organisationId) =>
        new()
        {
            ExternalOrganisationId = organisationId,
            OrganisationId = organisationDetails.Company.OrganisationId,
            OrganisationName = organisationDetails.Company.OrganisationName,
            OrganisationType = organisationDetails.Company.OrganisationTypeId.ToString(),
            CompaniesHouseNumber = organisationDetails.Company.CompaniesHouseNumber,
            BusinessAddress = new BusinessAddress
            {
                BuildingName = organisationDetails.Company.RegisteredAddress.BuildingName,
                BuildingNumber = organisationDetails.Company.RegisteredAddress.BuildingNumber,
                Street = organisationDetails.Company.RegisteredAddress.Street,
                County = organisationDetails.Company.RegisteredAddress.County,
                PostCode = organisationDetails.Company.RegisteredAddress.Postcode,
            },
            AdminUsers = organisationDetails.CompanyUserInformation.Where( u=>
                u.PersonRoleId == (int)PersonRole.Admin &&
                u.UserEnrolments.Any(e => e.ServiceRoleId == (int)ServiceRole.ProducerOther)
            ).ToList(),
            BasicUsers = organisationDetails.CompanyUserInformation.Where( u=>
                u.PersonRoleId == (int)PersonRole.Employee &&
                u.UserEnrolments.Any(e => e.ServiceRoleId == (int)ServiceRole.ProducerOther)
            ).ToList(),
            DelegatedUsers = organisationDetails.CompanyUserInformation.Where( u=>
                u.UserEnrolments.Any(e => e.ServiceRoleId == (int)ServiceRole.DelegatedPerson)
            ).ToList(),
            ApprovedUsersInformation = new ApprovedUserInformation()
            {
                ApprovedUsers = organisationDetails.CompanyUserInformation.Where( u=>
                    u.UserEnrolments.Any(e => e.ServiceRoleId == (int)ServiceRole.ApprovedPerson)
                ).ToList(),
                OrganisationName = organisationDetails.Company.OrganisationName,
                OrganisationExternalId = organisationId
            }
        };
}