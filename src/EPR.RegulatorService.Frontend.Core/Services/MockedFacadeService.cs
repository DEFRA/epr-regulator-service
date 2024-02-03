using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.MockedData.Registrations;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using EPR.RegulatorService.Frontend.Core.Enums;
using System.Security.Cryptography;

namespace EPR.RegulatorService.Frontend.Core.Services;

[ExcludeFromCodeCoverage]
public class MockedFacadeService : IFacadeService
{
    private const string ApprovedPerson = "ApprovedPerson";
    private const string DelegatedPerson = "DelegatedPerson";
    private const string Pending = "Pending";
    private const string Accepted = "Accepted";
    private const string Rejected = "Rejected";
    private readonly PaginationConfig _config;
    private static List<OrganisationApplications> _allItems = GenerateOrganisationApplications();
    private static List<Submission> _allSubmissions = GenerateOrganisationSubmissions();
    private static List<OrganisationSearchResult> _allSearchResults = GenerateOrganisationSearchResults();
    private static List<Registration> _allRegistrations = GenerateRegulatorRegistrations();

    public MockedFacadeService(IOptions<PaginationConfig> options)
    {
        _config = options.Value;
    }

    public async Task<string> GetTestMessageAsync()
    {
        return await Task.FromResult("Dummy test message from MockedFacadeService");
    }

    public Task<PaginatedList<OrganisationApplications>> GetUserApplicationsByOrganisation(
        string? applicationType,
        string? organisationName,
        int currentPage = 1)
    {
        if (currentPage > (int)Math.Ceiling(_allItems.Count / (double)_config.PageSize))
        {
            currentPage = 1;
        }

        bool hasApprovedPending = applicationType == ApprovedPerson;
        bool hasDelegatedPending = applicationType == DelegatedPerson;

        var results = _allItems;
        if (!string.IsNullOrEmpty(organisationName))
        {
            results = _allItems.Where(x =>
                x.OrganisationName.Contains(organisationName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (hasApprovedPending)
        {
            results = _allItems.Where(x =>
                    x.Enrolments.HasApprovedPending == hasApprovedPending &&
                    !x.Enrolments.HasDelegatePending)
                .ToList();
        }

        if (hasDelegatedPending)
        {
            results = _allItems.Where(x =>
                    x.Enrolments.HasDelegatePending == hasDelegatedPending &&
                    !x.Enrolments.HasApprovedPending)
                .ToList();
        }

        if (hasDelegatedPending && hasApprovedPending)
        {
            results = _allItems.Where(x =>
                x.Enrolments.HasDelegatePending == hasDelegatedPending &&
                x.Enrolments.HasApprovedPending == hasApprovedPending).ToList();
        }

        var response = new PaginatedList<OrganisationApplications>
        {
            Items = results
                .OrderByDescending(x => x.Enrolments.HasApprovedPending)
                .ThenBy(x => x.LastUpdate)
                .ThenBy(x => x.OrganisationName)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList(),
            CurrentPage = currentPage,
            TotalItems = results.Count,
            PageSize = (int)Math.Ceiling(results.Count / (double)_config.PageSize)
        };

        return Task.FromResult(response);
    }

    private static List<OrganisationApplications> GenerateOrganisationApplications()
    {
        var allItems = new List<OrganisationApplications>();
        var random = new Random();

        for (int i = 1; i <= 1000; i++)
        {
            var hasApprovedPending = random.Next(2) == 0;
            var hasDelegatedPending = !hasApprovedPending || random.Next(2) == 0;

            string organisationName = string.Empty;

            switch (i % 3)
            {
                case 0:
                    // Regular name
                    organisationName = $"Organisation {i} Ltd";
                    break;
                case 1:
                    // Extra long unbroken name
                    organisationName = $"Organisation{i}LtdReallyReallyReallyReallyReallyReallyReallyReallyLongName";
                    break;
                case 2:
                    // Extra long name with spaces
                    organisationName += $"Organisation{i}Ltd ReallyReallyReallyReallyReallyReallyReally Really Long Name";
                    break;
            }

            allItems.Add(new OrganisationApplications
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = organisationName,
                LastUpdate = DateTime.Now.AddDays(-random.Next(1, 15)),
                Enrolments = new()
                {
                    HasApprovedPending = hasApprovedPending, HasDelegatePending = hasDelegatedPending
                }
            });
        }

        return allItems;
    }

    private static List<Submission> GenerateOrganisationSubmissions()
    {
        var allItems = new List<Submission>();

        allItems.AddRange(MockedPendingSubmissions.GetMockedPendingSubmissions(1, 100));
        allItems.AddRange(MockedRejectedSubmissions.GetMockedRejectedSubmissions(101, 200));
        allItems.AddRange(MockedAcceptedSubmissions.GetMockedAcceptedSubmissions(201, 300));

        return allItems;
    }

    private static string GetRandomSixDigitNumber()
    {
        int number = RandomNumberGenerator.GetInt32(100000, 999999); // Generate a random 6-digit number

        return number.ToString();
    }

    private static List<OrganisationSearchResult> GenerateOrganisationSearchResults()
    {
        var allItems = new List<OrganisationSearchResult>();
        var random = RandomNumberGenerator.GetInt32(0, 2);

        for (int i = 1; i <= 1000; i++)
        {
            bool isComplianceScheme = random == 0;
            bool hasCompaniesHouseNumber = random == 0;
            string companiesHouseNumber = hasCompaniesHouseNumber ? $"NI{GetRandomSixDigitNumber()}" : "";
            string organisationId = GetRandomSixDigitNumber();

            allItems.Add(new OrganisationSearchResult
            {
                ExternalId = Guid.NewGuid(),
                OrganisationName = $"Organisation {i} Ltd",
                IsComplianceScheme = isComplianceScheme,
                CompanyHouseNumber = companiesHouseNumber,
                OrganisationId = organisationId
            });
        }

        return allItems;
    }

    public async Task<OrganisationEnrolments> GetOrganisationEnrolments(Guid organisationId)
    {
        var organisationEnrolments = MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails();
        return await Task.FromResult(organisationEnrolments);
    }

    public async Task<RegulatorCompanyDetailsModel> GetRegulatorCompanyDetails(Guid organisationId)
    {
        var organisationDetails = MockedOrganisationDetails.GetMockedOrganisationDetails();
        return await Task.FromResult(organisationDetails);
    }

    public async Task<EndpointResponseStatus> UpdateEnrolment(UpdateEnrolment updateEnrolment)
    {
        return await Task.FromResult(EndpointResponseStatus.Success);
    }

    public async Task<EndpointResponseStatus> SendEnrolmentEmails(EnrolmentDecisionRequest request)
    {
        return await Task.FromResult(EndpointResponseStatus.Success);
    }

    public async Task<EndpointResponseStatus> TransferOrganisationNation(
        OrganisationTransferNationRequest organisationNationTransfer) => EndpointResponseStatus.Success;

    public async Task<EndpointResponseStatus> EnrolInvitedUser(EnrolInvitedUserRequest request)
    {
        return await Task.FromResult(EndpointResponseStatus.Success);
    }

    public Task<HttpResponseMessage> GetUserAccountDetails()
    {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    public Task<PaginatedList<Submission>> GetOrganisationSubmissions(
        string? organisationName,
        string? organisationReference,
        string? organisationType,
        string[]? status,
        int currentPage = 1)
    {
        if (currentPage > (int)Math.Ceiling(_allSubmissions.Count / (double)_config.PageSize))
        {
            currentPage = 1;
        }

        var results = _allSubmissions
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(organisationName, organisationReference)
            .FilterByOrganisationType(organisationType)
            .FilterBySubmissionStatus(status).ToList();

        var response = new PaginatedList<Submission>
        {
            Items = results
                .OrderBy(x => x.Decision == Accepted)
                .ThenBy(x => x.Decision == Rejected)
                .ThenBy(x => x.Decision == Pending)
                .ThenBy(x => x.IsResubmission)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList(),
            CurrentPage = currentPage,
            TotalItems = results.Count,
            PageSize = _config.PageSize
        };

        return Task.FromResult(response);
    }

    public async Task<EndpointResponseStatus> SubmitPoMDecision(RegulatorPoMDecisionCreateRequest request) => EndpointResponseStatus.Success;

    public Task<PaginatedList<OrganisationSearchResult>> GetOrganisationBySearchTerm(string searchTerm, int currentPage = 1)
    {
        if (currentPage > (int)Math.Ceiling(_allItems.Count / (double)_config.PageSize))
        {
            currentPage = 1;
        }

        var results = _allSearchResults;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            results = _allSearchResults.Where(x =>
                x.OrganisationName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || x.OrganisationId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var response = new PaginatedList<OrganisationSearchResult>
        {
            Items = results
                .OrderByDescending(x => x.OrganisationName)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList(),
            CurrentPage = currentPage,
            TotalItems = results.Count,
            PageSize = (int)Math.Ceiling(results.Count / (double)_config.PageSize)
        };

        return Task.FromResult(response);
    }

    public Task<List<OrganisationUser>> GetProducerOrganisationUsersByOrganisationExternalId(Guid externalId)
    {
        var response = new List<OrganisationUser>()
        {
            new() {FirstName = "James", LastName = "Smith", Email = "jsmith@gmail.com", PersonExternalId = Guid.NewGuid()},
            new() {FirstName = "John", LastName = "Lewis", Email = "jlewis@gmail.com", PersonExternalId = Guid.NewGuid()},
            new() {FirstName = "Marie", LastName = "Harris", Email = "mharris@gmail.com", PersonExternalId = Guid.NewGuid()},
            new() {FirstName = "Ciara", LastName = "Dunne", Email = "cdunne@gmail.com", PersonExternalId = Guid.NewGuid()},
            new() {FirstName = "Peter", LastName = "Taylor", Email = "ptaylor@gmail.com", PersonExternalId = Guid.NewGuid()}
        };

        return Task.FromResult(response);
    }

    public async Task<EndpointResponseStatus> RemoveApprovedUser(RemoveApprovedUserRequest request) => EndpointResponseStatus.Success;
    public async Task<EndpointResponseStatus> RemoveApprovedUser(Guid connExternalId, Guid organisationId) => EndpointResponseStatus.Success;
    public async Task<EndpointResponseStatus> RejectSubmission() => EndpointResponseStatus.Success;

    public async Task<EndpointResponseStatus> AcceptSubmission() => EndpointResponseStatus.Success;

    private static List<Registration> GenerateRegulatorRegistrations()
    {
        var allItems = new List<Registration>();

        allItems.AddRange(MockedPendingRegistrations.GetMockedPendingRegistrations(1, 100));
        allItems.AddRange(MockedRejectedRegistrations.GetMockedRejectedRegistrations(101, 200));
        allItems.AddRange(MockedAcceptedRegistrations.GetMockedAcceptedRegistrations(201, 300));

        return allItems;
    }

    public async Task<PaginatedList<Registration>> GetRegulatorRegistrations(
        string? organisationName,
        string? organisationReference,
        OrganisationType? organisationType,
        string[]? status,
        int currentPage = 1)
    {
        if (currentPage > (int)Math.Ceiling(_allRegistrations.Count / (double)_config.PageSize))
        {
            currentPage = 1;
        }

        var results = _allRegistrations
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(organisationName, organisationReference)
            .FilterByOrganisationType(organisationType)
            .FilterByRegistrationStatus(status).ToList();

        var response = new PaginatedList<Registration>
        {
            Items = results
                .OrderBy(x => x.Decision == Accepted)
                .ThenBy(x => x.Decision == Rejected)
                .ThenBy(x => x.Decision == Pending)
                .ThenBy(x => x.RegistrationDate)
                .ThenBy(x => x.IsResubmission)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList(),
            CurrentPage = currentPage,
            TotalItems = results.Count,
            PageSize = _config.PageSize
        };

        return response;
    }

    public async Task<EndpointResponseStatus> AddRemoveApprovedUser(AddRemoveApprovedUserRequest request) => await Task.FromResult(EndpointResponseStatus.Success);
}