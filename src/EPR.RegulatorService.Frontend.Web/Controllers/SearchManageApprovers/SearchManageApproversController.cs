using EPR.Common.Authorization.Constants;
using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
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
        IConfiguration configuration,
        IFacadeService facadeService)
        : base(sessionManager, configuration)
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
        organisationDetails.CompanyUserInformation = ReshapeData(organisationDetails.CompanyUserInformation);

        var model = GetCompanyDetailsRequest(organisationDetails, session.RegulatorSession.OrganisationId.Value);
        var currentApprovedUser = model.ApprovedUsersInformation.ApprovedUsers.FirstOrDefault();

        session.AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
        {
            OrganisationName = model.OrganisationName,
            ExternalOrganisationId = model.ExternalOrganisationId,
            CurrentApprovedUserEmail = currentApprovedUser != null ? currentApprovedUser.Email : string.Empty
        };

        await AddCurrentPageAndSaveSession(session, PagePath.RegulatorCompanyDetail);
        SetBackLink(session, PagePath.RegulatorCompanyDetail);
        return View(model);
    }


    /// <summary>
    /// This method reshapes the company user information list.
    /// It essentially ensures that only single users exist, but could _actually_ have multiple enrolment records
    /// (as opposed to the original list of multiples of the same users with a single enrolment record)
    /// </summary>
    /// <param name="companyUserInformationList">Reshaped lits of CompanyUserInformation objects</param>
    private static List<CompanyUserInformation> ReshapeData(List<CompanyUserInformation> companyUserInformationList)
    {
        var newUserList = new List<CompanyUserInformation>();

        foreach (var companyUser in companyUserInformationList)
        {
            if (newUserList.Exists(a => a.Email == companyUser.Email))
            {
                newUserList.First(a => a.Email == companyUser.Email).UserEnrolments.AddRange(companyUser.UserEnrolments);
                newUserList.First(a => a.Email == companyUser.Email).IsEmployee = companyUser.IsEmployee;
                newUserList.First(a => a.Email == companyUser.Email).JobTitle = companyUser.JobTitle;
                continue;
            }

            newUserList.Add(companyUser);
        }

        return newUserList;
    }

    private static RegulatorCompanyDetailViewModel GetCompanyDetailsRequest(RegulatorCompanyDetailsModel organisationDetails, Guid organisationId) =>
        new()
        {
            IsComplianceScheme = organisationDetails.Company.IsComplianceScheme,
            ExternalOrganisationId = organisationId,
            OrganisationId = organisationDetails.Company.OrganisationId,
            OrganisationName = organisationDetails.Company.OrganisationName,
            OrganisationType = organisationDetails.Company.OrganisationType,
            CompaniesHouseNumber = organisationDetails.Company.CompaniesHouseNumber,
            BusinessAddress = new BusinessAddress
            {
                BuildingName = organisationDetails.Company.RegisteredAddress.BuildingName,
                BuildingNumber = organisationDetails.Company.RegisteredAddress.BuildingNumber,
                Street = organisationDetails.Company.RegisteredAddress.Street,
                County = organisationDetails.Company.RegisteredAddress.County,
                PostCode = organisationDetails.Company.RegisteredAddress.Postcode,
            },
            AdminUsers = organisationDetails.CompanyUserInformation.Where(u =>
                u.PersonRoleId == (int)PersonRole.Admin &&
                u.UserEnrolments.Exists(e => e.ServiceRoleId == (int)ServiceRole.ProducerOther) &&
                !u.UserEnrolments.Exists(e => e.EnrolmentStatusId == (int)Core.Enums.EnrolmentStatus.Nominated)
            ).ToList(),
            BasicUsers = organisationDetails.CompanyUserInformation.Where(u =>
                u.PersonRoleId == (int)PersonRole.Employee &&
                u.UserEnrolments.Exists(e => e.ServiceRoleId == (int)ServiceRole.ProducerOther)
            ).ToList(),
            DelegatedUsers = organisationDetails.CompanyUserInformation.Where(u =>
                u.UserEnrolments.Exists(e => e.ServiceRoleId == (int)ServiceRole.DelegatedPerson)
            ).ToList(),
            ApprovedUsersInformation = new ApprovedUserInformation()
            {
                ApprovedUsers = organisationDetails.CompanyUserInformation.Where(u =>
                    u.UserEnrolments.Exists(e => e.ServiceRoleId == (int)ServiceRole.ApprovedPerson)
                ).ToList(),
                OrganisationName = organisationDetails.Company.OrganisationName,
                OrganisationExternalId = organisationId
            }
        };
}