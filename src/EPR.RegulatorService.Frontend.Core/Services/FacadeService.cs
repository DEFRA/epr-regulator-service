using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

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

    private readonly string[] _scopes;
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly PaginationConfig _paginationConfig;
    private readonly FacadeApiConfig _facadeApiConfig;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FacadeService(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IOptions<PaginationConfig> paginationOptions,
        IOptions<FacadeApiConfig> facadeApiOptions)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _paginationConfig = paginationOptions.Value;
        _facadeApiConfig = facadeApiOptions.Value;
        _scopes = new[] { _facadeApiConfig.DownstreamScope };
    }

    private static readonly Dictionary<Type, string> _typeToEndpointMap = new()
    {
        { typeof(Submission), "PomSubmissions" },
        { typeof(Registration), "RegistrationSubmissions" }
    };

    public async Task<string> GetTestMessageAsync()
    {
        var response = await _httpClient.GetAsync("/api/test");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return response.ToString();
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

        var response = await _httpClient.PostAsJsonAsync(path, filters);

        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PaginatedList<RegistrationSubmissionOrganisationDetails>>() : null;
    }

    public async Task<RegistrationSubmissionOrganisationDetails> GetRegistrationSubmissionDetails(Guid submissionId)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[GetOrganisationRegistrationSubmissionDetailsPath].Replace("{0}", submissionId.ToString());

        var response = await _httpClient.GetAsync(path);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<RegistrationSubmissionOrganisationDetails>() : null;
    }

    public async Task<EndpointResponseStatus> SubmitRegulatorRegistrationDecisionAsync(RegulatorDecisionRequest request)
    {
        await PrepareAuthenticatedClient();

        string path = _facadeApiConfig.Endpoints[OrgsanisationRegistrationSubmissionDecisionPath];
        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

}