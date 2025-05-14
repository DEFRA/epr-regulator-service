using CsvHelper.Configuration;
using CsvHelper;

using EPR.RegulatorService.Frontend.Core.ClassMaps;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.MockedData.Registrations;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
namespace EPR.RegulatorService.Frontend.Core.Services;

[ExcludeFromCodeCoverage]
public partial class MockedFacadeService(IOptions<PaginationConfig> options) : IFacadeService
{
    private const string ApprovedPerson = "ApprovedPerson";
    private const string DelegatedPerson = "DelegatedPerson";
    private const string Pending = "Pending";
    private const string Accepted = "Accepted";
    private const string Rejected = "Rejected";
    private readonly PaginationConfig _config = options.Value;
    private static readonly List<OrganisationApplications> _allItems = GenerateOrganisationApplications();
    private static readonly List<Submission> _allSubmissions = GenerateOrganisationSubmissions();
    private static readonly List<OrganisationSearchResult> _allSearchResults = GenerateOrganisationSearchResults();
    private static readonly List<Registration> _allRegistrations = GenerateRegulatorRegistrations();

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
            items = results
                .OrderByDescending(x => x.Enrolments.HasApprovedPending)
                .ThenBy(x => x.LastUpdate)
                .ThenBy(x => x.OrganisationName)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList(),
            currentPage = currentPage,
            totalItems = results.Count,
            pageSize = (int)Math.Ceiling(results.Count / (double)_config.PageSize)
        };

        return Task.FromResult(response);
    }

    private static List<OrganisationApplications> GenerateOrganisationApplications()
    {
        var allItems = new List<OrganisationApplications>();

        for (int i = 1; i <= 1000; i++)
        {
            bool hasApprovedPending = (i % 2) == 0;
            bool hasDelegatedPending = (i % 4) < 2;

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
                default:
                    organisationName = $"Organisation {i} Ltd With an even more and even longer meaningless name that will never fit in any view";
                    break;
            }

            allItems.Add(new OrganisationApplications
            {
                OrganisationId = Guid.NewGuid(),
                OrganisationName = organisationName,
                LastUpdate = DateTime.Now.AddDays(-(i % 15)),
                Enrolments = new()
                {
                    HasApprovedPending = hasApprovedPending,
                    HasDelegatePending = hasDelegatedPending
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

        return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static List<OrganisationSearchResult> GenerateOrganisationSearchResults()
    {
        var allItems = new List<OrganisationSearchResult>();
        int random = RandomNumberGenerator.GetInt32(0, 2);

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
            items = results
                .OrderByDescending(x => x.OrganisationName)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList(),
            currentPage = currentPage,
            totalItems = results.Count,
            pageSize = (int)Math.Ceiling(results.Count / (double)_config.PageSize)
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
    public static async Task<EndpointResponseStatus> RemoveApprovedUser(Guid connExternalId, Guid organisationId) => EndpointResponseStatus.Success;

    public async Task<EndpointResponseStatus> SubmitRegistrationDecision(
        RegulatorRegistrationDecisionCreateRequest request) => EndpointResponseStatus.Success;

    public async Task<PaginatedList<T>> GetOrganisationSubmissions<T>(string? organisationName, string? organisationReference,
        OrganisationType? organisationType, string[]? status, int[]? submissionYears, string[]? submissionPeriods, int currentPage = 1) where T : AbstractSubmission
    {
        var items = typeof(T) == typeof(Submission)
            ? _allSubmissions.Cast<AbstractSubmission>().ToList()
            : _allRegistrations.Cast<AbstractSubmission>().ToList();

        if (currentPage > (int)Math.Ceiling(_allSubmissions.Count / (double)_config.PageSize))
        {
            currentPage = 1;
        }

        var results = items
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(organisationName, organisationReference)
            .FilterByOrganisationType(organisationType)
            .FilterByStatus(status)
            .FilterBySubmissionYears(submissionYears)
            .FilterBySubmissionPeriods(submissionPeriods)
            .ToList();

        var response = new PaginatedList<T>
        {
            items = await FilterSubmissions<T>(results, currentPage),
            currentPage = currentPage,
            totalItems = results.Count,
            pageSize = _config.PageSize
        };

        return response;
    }

    public async Task<Stream> GetRegistrationSubmissionsCsv(GetRegistrationSubmissionsCsvRequest request)
    {
        var submissions = _allRegistrations.Cast<AbstractSubmission>()
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(request.SearchOrganisationName, request.SearchOrganisationId)
            .FilterByOrganisationType(GetFilterOrganisationType(request.IsDirectProducerChecked, request.IsComplianceSchemeChecked))
            .FilterByStatus(GetFilterStatuses(request.IsPendingRegistrationChecked, request.IsAcceptedRegistrationChecked, request.IsRejectedRegistrationChecked))
            .FilterBySubmissionYears(request.SearchSubmissionYears)
            .FilterBySubmissionPeriods(request.SearchSubmissionPeriods)
            .Cast<Registration>()
            .Select(x => new SubmissionCsvModel
            {
                Organisation = FormatOrganisationName(x.OrganisationName, x.OrganisationType),
                OrganisationId = x.OrganisationReference,
                SubmissionDate = x.RegistrationDate.ToString("d MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                SubmissionPeriod = x.SubmissionPeriod,
                Status = x.Decision
            }).ToList();


        return await CreateSubmissionsCsv(submissions);
    }

    public async Task<Stream> GetPackagingSubmissionsCsv(GetPackagingSubmissionsCsvRequest request)
    {
        var submissions = _allSubmissions.Cast<AbstractSubmission>()
            .AsQueryable()
            .FilterByOrganisationNameAndOrganisationReference(request.SearchOrganisationName, request.SearchOrganisationId)
            .FilterByOrganisationType(GetFilterOrganisationType(request.IsDirectProducerChecked, request.IsComplianceSchemeChecked))
            .FilterByStatus(GetFilterStatuses(request.IsPendingSubmissionChecked, request.IsAcceptedSubmissionChecked, request.IsRejectedSubmissionChecked))
            .FilterBySubmissionYears(request.SearchSubmissionYears)
            .FilterBySubmissionPeriods(request.SearchSubmissionPeriods)
            .Cast<Submission>()
            .AsEnumerable()
            .Select(x => new SubmissionCsvModel
            {
                Organisation = FormatOrganisationName(x.OrganisationName, x.OrganisationType),
                OrganisationId = x.OrganisationReference,
                SubmissionDate = x.SubmittedDate.ToString("d MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                SubmissionPeriod = !string.IsNullOrEmpty(x.ActualSubmissionPeriod)
                    ? string.Join(", ", x.ActualSubmissionPeriod.Split(","))
                    : x.SubmissionPeriod,
                Status = x.Decision
            }).ToList();


        return await CreateSubmissionsCsv(submissions);
    }

    private static List<Registration> GenerateRegulatorRegistrations()
    {
        var allItems = new List<Registration>();

        allItems.AddRange(MockedPendingRegistrations.GetMockedPendingRegistrations(1, 100));
        allItems.AddRange(MockedRejectedRegistrations.GetMockedRejectedRegistrations(101, 200));
        allItems.AddRange(MockedAcceptedRegistrations.GetMockedAcceptedRegistrations(201, 300));

        return allItems;
    }

    public async Task<List<T>> FilterSubmissions<T>(
        IList<AbstractSubmission> items, int currentPage) where T : AbstractSubmission
    {
        if (typeof(T) == typeof(Submission))
        {
            var filteredItems = items.OfType<Submission>()
                .OrderBy(x => x.Decision == Accepted)
                .ThenBy(x => x.Decision == Rejected)
                .ThenBy(x => x.Decision == Pending)
                .ThenBy(x => x.IsResubmission)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList();

            return filteredItems.Cast<T>().ToList();
        }

        if (typeof(T) == typeof(Registration))
        {
            var filteredItems = items.OfType<Registration>()
                .OrderBy(x => x.Decision == Accepted)
                .ThenBy(x => x.Decision == Rejected)
                .ThenBy(x => x.Decision == Pending)
                .ThenBy(x => x.RegistrationDate)
                .ThenBy(x => x.IsResubmission)
                .Skip((currentPage - 1) * _config.PageSize)
                .Take(_config.PageSize)
                .ToList();

            return filteredItems.Cast<T>().ToList();
        }

        return [];
    }

    public async Task<EndpointResponseStatus> AddRemoveApprovedUser(AddRemoveApprovedUserRequest request) => await Task.FromResult(EndpointResponseStatus.Success);

    public async Task<HttpResponseMessage> GetFileDownload(FileDownloadRequest request)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("This is a mock file content")
        };

        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "mockfile.txt"
        };

        return await Task.FromResult(response);
    }

    public async Task<PaginatedList<RegistrationSubmissionOrganisationDetails>> GetRegistrationSubmissions(RegistrationSubmissionsFilterModel filters)
    {
        var response = new PaginatedList<RegistrationSubmissionOrganisationDetails>
        {
            items = [],
            currentPage = filters.PageNumber.Value,
            totalItems = 0,
            pageSize = filters.PageSize.Value
        };

        return response;
    }

    public async Task<PaginatedList<RegistrationSubmissionOrganisationDetails>> GetTransformedRegistrationSubmissions(RegistrationSubmissionsFilterModel filters)
        => await GetRegistrationSubmissions(filters);

    public async Task<RegistrationSubmissionOrganisationDetails> GetRegistrationSubmissionDetails(Guid submissionId)
    {
        RegistrationSubmissionOrganisationDetails objRet = null;

        return await Task.FromResult(objRet);
    }

    public async Task<RegistrationSubmissionOrganisationDetails> GetTransformedRegistrationSubmissionDetails(Guid submissionId)
        => await GetRegistrationSubmissionDetails(submissionId);

    public async Task<EndpointResponseStatus> SubmitRegulatorRegistrationDecisionAsync(
        RegulatorDecisionRequest request) => await Task.FromResult(EndpointResponseStatus.Success);

    public async Task SubmitRegistrationFeePaymentAsync(
        FeePaymentRequest request) => await Task.FromResult(EndpointResponseStatus.Success);

    public async Task SubmitPackagingDataResubmissionFeePaymentEventAsync(
        FeePaymentRequest request) => await Task.FromResult(EndpointResponseStatus.Success);

    public async Task<PomPayCalParametersResponse> GetPomPayCalParameters(
       Guid submissionId, Guid? complianceSchemeId) => await Task.FromResult(new PomPayCalParametersResponse());

    private static OrganisationType? GetFilterOrganisationType(bool isDirectProducerChecked, bool isComplianceSchemeChecked)
    {
        if (isDirectProducerChecked && !isComplianceSchemeChecked)
        {
            return OrganisationType.DirectProducer;
        }

        if (isComplianceSchemeChecked && !isDirectProducerChecked)
        {
            return OrganisationType.ComplianceScheme;
        }

        return null;
    }

    private static string[] GetFilterStatuses(bool isPendingStatusChecked, bool isAcceptedStatusChecked, bool isRejectedStatusChecked)
    {
        var submissionStatuses = new List<string>();

        if (isPendingStatusChecked)
        {
            submissionStatuses.Add(nameof(OrganisationSubmissionStatus.Pending));
        }

        if (isAcceptedStatusChecked)
        {
            submissionStatuses.Add(nameof(OrganisationSubmissionStatus.Accepted));
        }

        if (isRejectedStatusChecked)
        {
            submissionStatuses.Add(nameof(OrganisationSubmissionStatus.Rejected));
        }

        return submissionStatuses.ToArray();
    }

    private static string FormatOrganisationName(string organisationName, OrganisationType organisationType)
    {
        return organisationName + " (" + (organisationType == OrganisationType.DirectProducer ? "Direct Producer" : "Compliance Scheme") + ")";
    }

    private static async Task<Stream> CreateSubmissionsCsv(IEnumerable<SubmissionCsvModel> submissions)
    {
        var stream = new MemoryStream();

        await using (var writer = new StreamWriter(stream, leaveOpen: true, encoding: Encoding.UTF8))
        {
            await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Context.RegisterClassMap<SubmissionClassMap>();
                csv.WriteRecordsAsync(submissions);
            }

            await writer.FlushAsync();
        }

        stream.Position = 0;
        return stream;
    }
}