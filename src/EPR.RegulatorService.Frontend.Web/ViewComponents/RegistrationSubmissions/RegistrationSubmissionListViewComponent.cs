using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EPR.RegulatorService.Frontend.Web.ViewComponents;

[ExcludeFromCodeCoverage]
public class RegistrationSubmissionListViewComponent : ViewComponent
{
    private readonly IFacadeService _facadeService;

    public RegistrationSubmissionListViewComponent(IFacadeService facadeService)
    {
        _facadeService = facadeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(RegistrationSubmissionsListViewModel request)
    {
        // Uncomment to use real data
        // var pagedOrganisationRegistrations = await _facadeService.GetRegistrationSubmissions(request.RegistrationsFilterModel);

        // Use test data for development
        var pagedOrganisationRegistrations = GetTestData();

        var dataGrid = new DataGrid<ISubmissionDetails>
        {
            Data = pagedOrganisationRegistrations.items.Select(x => new RegistrationSubmissionDetailsViewModel
            {
                OrganisationName = x.OrganisationName,
                OrganisationReference = x.OrganisationReference,
                RegistrationDateTime = x.SubmissionDate,
                RegistrationYear = x.RelevantYear,
                Status = x.SubmissionStatus,
                OrganisationType = x.OrganisationType
            }).ToList(),

            Headers = new List<DataGridHeader<ISubmissionDetails>>
        {
            new DataGridHeader<ISubmissionDetails>
            {
                DisplayName = "Company",
                ValueExpression = item => item.OrganisationName,
                Class = "epr-width-thirty-percent"
            },
            new DataGridHeader<ISubmissionDetails>
            {
                DisplayName = "Company ID",
                ValueExpression = item => item.OrganisationReference,
                Class = "epr-width-thirty-percent"
            },
            new DataGridHeader<ISubmissionDetails>
            {
                DisplayName = "Application Date",
                ValueExpression = item => item.RegistrationDateTime.ToString("dd MMMM yyyy"),
                Class = "epr-width-thirty-percent"
            },
            new DataGridHeader<ISubmissionDetails>
            {
                DisplayName = "Year",
                ValueExpression = item => item.RegistrationYear,
                Class = "epr-width-ten-percent"
            },
            new DataGridHeader<ISubmissionDetails>
            {
                DisplayName = "Status",
                ValueExpression = item => item.Status.GetDescription(),
                Class = "epr-width-ten-percent tag"
            }
        }
        };

        return View(dataGrid);
    }

    /// <summary>
    /// Generates test data for Registration Submissions.
    /// </summary>
    /// <returns>PaginatedList of RegistrationSubmissionOrganisationDetails.</returns>
    private PaginatedList<RegistrationSubmissionOrganisationDetails> GetTestData()
    {
        return new PaginatedList<RegistrationSubmissionOrganisationDetails>
        {
            items = new List<RegistrationSubmissionOrganisationDetails>
            {
                new RegistrationSubmissionOrganisationDetails
                {
                    SubmissionId = Guid.NewGuid(),
                    OrganisationId = Guid.NewGuid(),
                    OrganisationName = "AQZ LIMITED",
                    OrganisationReference = "105832",
                    RegistrationReferenceNumber = "REG-2025-001",
                    ApplicationReferenceNumber = "APP-2025-001",
                    SubmissionDate = new DateTime(2025, 1, 14),
                    RelevantYear = 2025,
                    SubmissionStatus = RegistrationSubmissionStatus.Pending,
                    OrganisationType = RegistrationSubmissionOrganisationType.large
                },
                new RegistrationSubmissionOrganisationDetails
                {
                    SubmissionId = Guid.NewGuid(),
                    OrganisationId = Guid.NewGuid(),
                    OrganisationName = "NEWPORT ASSET HOLDINGS LIMITED",
                    OrganisationReference = "105910",
                    RegistrationReferenceNumber = "REG-2025-002",
                    ApplicationReferenceNumber = "APP-2025-002",
                    SubmissionDate = new DateTime(2025, 1, 20),
                    RelevantYear = 2025,
                    SubmissionStatus = RegistrationSubmissionStatus.Pending,
                    OrganisationType = RegistrationSubmissionOrganisationType.large
                },
                new RegistrationSubmissionOrganisationDetails
                {
                    SubmissionId = Guid.NewGuid(),
                    OrganisationId = Guid.NewGuid(),
                    OrganisationName = "THE OFFICE OF BORIS JOHNSON LIMITED",
                    OrganisationReference = "105780",
                    RegistrationReferenceNumber = "REG-2025-003",
                    ApplicationReferenceNumber = "APP-2025-003",
                    SubmissionDate = new DateTime(2025, 2, 3),
                    RelevantYear = 2025,
                    SubmissionStatus = RegistrationSubmissionStatus.Granted,
                    OrganisationType = RegistrationSubmissionOrganisationType.large
                }
            },
            currentPage = 1,
            totalItems = 3,
            pageSize = 10
        };
    }
}
