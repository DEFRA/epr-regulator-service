using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using CsvHelper;
using CsvHelper.Configuration;

using EPR.RegulatorService.Frontend.Core.ClassMaps;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
namespace EPR.RegulatorService.Frontend.Core.Services;

public class FacadeService : IFacadeService
{
    private const string UpdateEnrolmentEndpointKey = "UpdateEnrolment";
    private const string PendingApplicationsEndpointKey = "PendingApplications";
    private const string PomSubmissionDecision = "PomSubmissionDecision";
    private const string SendEnrolmentUpdatedEmailsEndpointKey = "SendEnrolmentUpdatedEmails";
    private const string UserAccountEndpointKey = "UserAccounts";
    private const string OrganisationEnrolmentsPath = "OrganisationEnrolmentsPath";
    private const string OrganisationTransferNationPath = "OrganisationTransferNationPath";
    private const string EnrolInvitedUserEndpointKey = "EnrolInvitedUser";
    private const string OrganisationDetailsPath = "OrganisationDetails";
    private const string OrganisationsSearchPath = "OrganisationsSearchPath";
    private const string GetOrganisationUsersByOrganisationExternalIdPath = "GetOrganisationUsersByOrganisationExternalIdPath";
    private const string OrganisationsRemoveApprovedUserPath = "OrganisationsRemoveApprovedUser";
    private const string AddRemoveApprovedUserPath = "AddRemoveApprovedUser";
    private const string RegistrationSubmissionDecisionPath = "RegistrationSubmissionDecisionPath";
    private const string OrgsanisationRegistrationSubmissionDecisionPath = "OrganisationRegistrationSubmissionDecisionPath";
    private const string FileDownloadPath = "FileDownload";
    private const string GetOrganisationRegistrationSubmissionDetailsPath = "GetOrganisationRegistrationSubmissionDetailsPath";
    private const string GetOrganisationRegistationSubmissionsPath = "GetOrganisationRegistrationSubmissionsPath";
    private const string SubmitRegistrationFeePaymentPath = "SubmitRegistrationFeePaymentPath";
    private const string PackagingDataResubmissionFeePaymentPath = "PackagingDataResubmissionFeePaymentPath";
    private const string GetPomResubmissionPaycalParameters = "GetPomResubmissionPaycalParameters";

    private readonly string[] _scopes;
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly PaginationConfig _paginationConfig;
    private readonly FacadeApiConfig _facadeApiConfig;
    private readonly ILogger<FacadeService> _logger;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Action<ILogger, string, Exception?> _logFacadeServiceError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(FacadeService)),
            "An error occurred while processing a message: {ErrorMessage}");

    public FacadeService(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IOptions<PaginationConfig> paginationOptions,
        IOptions<FacadeApiConfig> facadeApiOptions,
        ILogger<FacadeService> logger)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _paginationConfig = paginationOptions.Value;
        _facadeApiConfig = facadeApiOptions.Value;
        _scopes = [_facadeApiConfig.DownstreamScope];
        _logger = logger;
    }

    private static readonly Dictionary<Type, string> _typeToEndpointMap = new()
    {
        { typeof(Submission), "PomSubmissions" },
        { typeof(Registration), "RegistrationSubmissions" }
    };

    public async Task<string> GetTestMessageAsync()
    {
        var response = await _httpClient.GetAsync("/api/test");
        return response.StatusCode == HttpStatusCode.OK ? await response.Content.ReadAsStringAsync() : response.ToString();
    }

    public async Task<PaginatedList<OrganisationApplications>> GetUserApplicationsByOrganisation(
        string? applicationType,
        string? organisationName,
        int currentPage = 1)
    {
        await PrepareAuthenticatedClient();

        var query = new Dictionary<string, string>
        {
            ["currentPage"] = currentPage.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["pageSize"] = _paginationConfig.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        if (!string.IsNullOrEmpty(organisationName))
        {
            query["organisationName"] = organisationName;
        }

        if (!string.IsNullOrEmpty(applicationType))
        {
            query["applicationType"] = applicationType;
        }

        string queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        string path = $"{_facadeApiConfig.Endpoints[PendingApplicationsEndpointKey]}?{queryString}";

        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedList<OrganisationApplications>>();
    }

    public async Task<OrganisationEnrolments> GetOrganisationEnrolments(Guid organisationId)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[OrganisationEnrolmentsPath]
            .Replace("{0}", organisationId.ToString());

        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrganisationEnrolments>();
    }

    public async Task<RegulatorCompanyDetailsModel> GetRegulatorCompanyDetails(Guid organisationId)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[OrganisationDetailsPath]
            .Replace("{0}", organisationId.ToString());

        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();

        string result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RegulatorCompanyDetailsModel>(result, _jsonSerializerOptions);
    }

    public async Task<EndpointResponseStatus> TransferOrganisationNation(
        OrganisationTransferNationRequest organisationNationTransfer)
    {
        await PrepareAuthenticatedClient();
        string path = _facadeApiConfig.Endpoints[OrganisationTransferNationPath];
        var response = await _httpClient.PostAsJsonAsync(path, organisationNationTransfer);
        return response.StatusCode == HttpStatusCode.NoContent
            ? EndpointResponseStatus.Success
            : throw (new Exception(response.Content.ToString()));
    }

    public async Task<EndpointResponseStatus> UpdateEnrolment(UpdateEnrolment updateEnrolment)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[UpdateEnrolmentEndpointKey];
        var response = await _httpClient.PostAsJsonAsync(path, updateEnrolment);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<EndpointResponseStatus> SendEnrolmentEmails(EnrolmentDecisionRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[SendEnrolmentUpdatedEmailsEndpointKey];
        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.StatusCode == HttpStatusCode.OK ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<EndpointResponseStatus> EnrolInvitedUser(EnrolInvitedUserRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[EnrolInvitedUserEndpointKey];
        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.StatusCode == HttpStatusCode.NoContent ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<HttpResponseMessage> GetUserAccountDetails()
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[UserAccountEndpointKey];

        var response = await _httpClient.GetAsync(path);

        return response;
    }

    public async Task<PaginatedList<T>> GetOrganisationSubmissions<T>(
        string? organisationName,
        string? organisationReference,
        OrganisationType? organisationType,
        string[]? status,
        int[]? submissionYears,
        string[]? submissionPeriods,
        int currentPage = 1) where T : AbstractSubmission
    {
        await PrepareAuthenticatedClient();

        var query = new Dictionary<string, string>
        {
            ["pageNumber"] = currentPage.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["pageSize"] = _paginationConfig.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        if (!string.IsNullOrEmpty(organisationName))
        {
            query["organisationName"] = organisationName;
        }

        if (!string.IsNullOrEmpty(organisationReference))
        {
            query["organisationReference"] = organisationReference;
        }

        if (organisationType.HasValue)
        {
            query["organisationType"] = organisationType.ToString();
        }
        else
        {
            query["organisationType"] = "All";
        }

        if (status?.Length > 0)
        {
            query["statuses"] = string.Join(',', status);
        }

        if (submissionYears?.Length > 0)
        {
            query["submissionYears"] = string.Join(',', submissionYears);
        }

        if (submissionPeriods?.Length > 0)
        {
            query["submissionPeriods"] = string.Join(',', submissionPeriods);
        }

        string queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        string endpointKey = _typeToEndpointMap[typeof(T)];
        string path = $"{_facadeApiConfig.Endpoints[endpointKey]}?{queryString}";

        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedList<T>>();
    }

    public async Task<Stream> GetRegistrationSubmissionsCsv(GetRegistrationSubmissionsCsvRequest request)
    {
        var submissions = (
            await GetAllOrganisationSubmissions<Registration>(
                request.SearchOrganisationName,
                request.SearchOrganisationId,
                GetFilterOrganisationType(request.IsDirectProducerChecked, request.IsComplianceSchemeChecked),
                GetFilterStatuses(request.IsPendingRegistrationChecked, request.IsAcceptedRegistrationChecked, request.IsRejectedRegistrationChecked),
                request.SearchSubmissionYears,
                request.SearchSubmissionPeriods)
            ).Select(x => new SubmissionCsvModel
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
        var submissions = (
            await GetAllOrganisationSubmissions<Submission>(
                request.SearchOrganisationName,
                request.SearchOrganisationId,
                GetFilterOrganisationType(request.IsDirectProducerChecked, request.IsComplianceSchemeChecked),
                GetFilterStatuses(request.IsPendingSubmissionChecked, request.IsAcceptedSubmissionChecked, request.IsRejectedSubmissionChecked),
                request.SearchSubmissionYears,
                request.SearchSubmissionPeriods)
            ).Select(x => new SubmissionCsvModel
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

    public async Task<EndpointResponseStatus> SubmitPoMDecision(RegulatorPoMDecisionCreateRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[PomSubmissionDecision];
        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<PaginatedList<OrganisationSearchResult>> GetOrganisationBySearchTerm(string searchTerm, int currentPage = 1)
    {
        await PrepareAuthenticatedClient();

        string path = string.Format(System.Globalization.CultureInfo.InvariantCulture, _facadeApiConfig.Endpoints[OrganisationsSearchPath], currentPage, _paginationConfig.PageSize, searchTerm);
        var response = await _httpClient.GetAsync(path);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedList<OrganisationSearchResult>>();
    }

    public async Task<List<OrganisationUser>> GetProducerOrganisationUsersByOrganisationExternalId(Guid externalId)
    {
        await PrepareAuthenticatedClient();

        string path = string.Format(System.Globalization.CultureInfo.InvariantCulture, _facadeApiConfig.Endpoints[GetOrganisationUsersByOrganisationExternalIdPath], externalId);

        var response = await _httpClient.GetAsync(path);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<OrganisationUser>>();
    }

    public async Task<EndpointResponseStatus> RemoveApprovedUser(RemoveApprovedUserRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = string.Format(System.Globalization.CultureInfo.InvariantCulture, _facadeApiConfig.Endpoints[OrganisationsRemoveApprovedUserPath],
            request.RemovedConnectionExternalId, request.OrganisationId, request.PromotedPersonExternalId);

        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<EndpointResponseStatus> AddRemoveApprovedUser(AddRemoveApprovedUserRequest request)
    {
        await PrepareAuthenticatedClient();
        string path = _facadeApiConfig.Endpoints[AddRemoveApprovedUserPath];

        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<EndpointResponseStatus> SubmitRegistrationDecision(RegulatorRegistrationDecisionCreateRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[RegistrationSubmissionDecisionPath];
        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<HttpResponseMessage> GetFileDownload(FileDownloadRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = string.Format(CultureInfo.InvariantCulture, _facadeApiConfig.Endpoints[FileDownloadPath]);

        var response = await _httpClient.PostAsJsonAsync(path, request);

        return await Task.FromResult<HttpResponseMessage>(response);
    }

    private async Task PrepareAuthenticatedClient()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_facadeApiConfig.BaseUrl);
            string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Constants.Bearer, accessToken);
        }
    }

    public async Task<PaginatedList<RegistrationSubmissionOrganisationDetails>> GetRegistrationSubmissions(RegistrationSubmissionsFilterModel filters)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[GetOrganisationRegistationSubmissionsPath];

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(path, filters);

        if (response.IsSuccessStatusCode)
        {
            var commonData = await ReadRequiredJsonContent(response.Content);
            var responseData = commonData.items.Select(x => (RegistrationSubmissionOrganisationDetails)x).ToList();

            return new PaginatedList<RegistrationSubmissionOrganisationDetails>
            {
                items = responseData,
                currentPage = commonData.currentPage,
                totalItems = commonData.totalItems,
                pageSize = commonData.pageSize
            };
        }
        else
        {
            return new PaginatedList<RegistrationSubmissionOrganisationDetails>
            {
                items = [],
                currentPage = 1,
                totalItems = 0,
                pageSize = 20
            };
        }

        return null;
    }

    public static async Task<PaginatedList<OrganisationRegistrationSubmissionSummaryResponse>> ReadRequiredJsonContent(HttpContent content)
    {
        string jsonContent = await content.ReadAsStringAsync();

        try
        {
            var response = JsonSerializer.Deserialize<PaginatedList<OrganisationRegistrationSubmissionSummaryResponse>>(jsonContent, _jsonSerializerOptions);

            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Cannot parse data from Facade for Submission Summaries", ex);
        }
    }

    public async Task<RegistrationSubmissionOrganisationDetails> GetRegistrationSubmissionDetails(Guid submissionId)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[GetOrganisationRegistrationSubmissionDetailsPath].Replace("{0}", submissionId.ToString());

        var response = await _httpClient.GetAsync(path);

        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        RegistrationSubmissionOrganisationDetailsResponse result = ConvertCommonDataToFE(content);

        return result;
    }

    private static RegistrationSubmissionOrganisationDetailsResponse ConvertCommonDataToFE(string content)
    {
        try
        {
            var response = JsonSerializer.Deserialize<RegistrationSubmissionOrganisationDetailsResponse>(content, _jsonSerializerOptions);
            response.SubmissionDetails.StatusPendingDate = response.StatusPendingDate;
            response.SubmissionDetails.ResubmissionDecisionDate = response.RegulatorResubmissionDecisionDate;
            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Cannot parse data from Facade for Submission Details", ex);
        }
    }

    public async Task<EndpointResponseStatus> SubmitRegulatorRegistrationDecisionAsync(RegulatorDecisionRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[OrgsanisationRegistrationSubmissionDecisionPath];
        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task SubmitRegistrationFeePaymentAsync(FeePaymentRequest request)
    {
        try
        {
            await PrepareAuthenticatedClient();

            string path = _facadeApiConfig.Endpoints[SubmitRegistrationFeePaymentPath];
            var response = await _httpClient.PostAsJsonAsync(path, request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logFacadeServiceError.Invoke(_logger, $"Exception occurred while submitting registration fee payment {nameof(FacadeService)}.{nameof(SubmitRegistrationFeePaymentAsync)}", ex);
        }
    }

    public async Task SubmitPackagingDataResubmissionFeePaymentEventAsync(FeePaymentRequest request)
    {
        try
        {
            await PrepareAuthenticatedClient();

            string path = _facadeApiConfig.Endpoints[PackagingDataResubmissionFeePaymentPath];
            var response = await _httpClient.PostAsJsonAsync(path, request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            _logFacadeServiceError.Invoke(
                _logger,
                $"Exception occurred while submitting packaging data resubmission fee payment {nameof(FacadeService)}.{nameof(SubmitPackagingDataResubmissionFeePaymentEventAsync)}",
                exception);
        }
    }

    public async Task<PomPayCalParametersResponse> GetPomPayCalParameters(Guid submissionId, Guid? complianceSchemeId)
    {
        try
        {
            await PrepareAuthenticatedClient();

            string url = $"{_facadeApiConfig.Endpoints[GetPomResubmissionPaycalParameters]}/{submissionId}";
            if (complianceSchemeId.HasValue)
            {
                url = $"{url}?ComplianceSchemeId={complianceSchemeId}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            var payCalDetailsResponse = JsonSerializer.Deserialize<PomPayCalParametersResponse>(content);
            return payCalDetailsResponse;
        }
        catch (Exception exception)
        {
            _logFacadeServiceError.Invoke(
                _logger,
                $"Exception occurred while retrieving pom resubmission pay cal parameters {nameof(FacadeService)}.{nameof(GetPomPayCalParameters)}",
                exception);

            return default;
        }
    }

    private async Task<List<T>> GetAllOrganisationSubmissions<T>(
        string? organisationName,
        string? organisationReference,
        OrganisationType? organisationType,
        string[]? status,
        int[]? submissionYears,
        string[]? submissionPeriods) where T : AbstractSubmission
    {
        const int pageSize = 200;

        await PrepareAuthenticatedClient();

        var query = new Dictionary<string, string>
        {
            ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
        };

        if (!string.IsNullOrEmpty(organisationName))
        {
            query["organisationName"] = organisationName;
        }

        if (!string.IsNullOrEmpty(organisationReference))
        {
            query["organisationReference"] = organisationReference;
        }

        if (organisationType.HasValue)
        {
            query["organisationType"] = organisationType.ToString();
        }
        else
        {
            query["organisationType"] = "All";
        }

        if (status?.Length > 0)
        {
            query["statuses"] = string.Join(',', status);
        }

        if (submissionYears?.Length > 0)
        {
            query["submissionYears"] = string.Join(',', submissionYears);
        }

        if (submissionPeriods?.Length > 0)
        {
            query["submissionPeriods"] = string.Join(',', submissionPeriods);
        }

        string queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        string endpointKey = _typeToEndpointMap[typeof(T)];
        string path = $"{_facadeApiConfig.Endpoints[endpointKey]}?{queryString}";

        var submissions = new List<T>();

        var response = await _httpClient.GetAsync($"{path}&pageNumber={1}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<T>>();

        int totalPages = result.TotalPages;

        if (result.items.Count > 0)
        {
            submissions.AddRange(result.items);
        }

        if (totalPages > 1)
        {
            for (int i = 2; i <= totalPages; i++)
            {
                response = await _httpClient.GetAsync($"{path}&pageNumber={i}");
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadFromJsonAsync<PaginatedList<T>>();

                submissions.AddRange(result.items);
            }
        }

        return submissions;
    }

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