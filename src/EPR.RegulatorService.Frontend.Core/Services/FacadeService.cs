using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Text.Json;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;

namespace EPR.RegulatorService.Frontend.Core.Services;

using Enums;

public class FacadeService : IFacadeService
{
    private const string UpdateEnrolmentEndpointKey = "UpdateEnrolment";
    private const string PendingApplicationsEndpointKey = "PendingApplications";
    private const string PomSubmissionsEndpointKey = "PomSubmissions";
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
    private readonly string[] _scopes;
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly PaginationConfig _paginationConfig;
    private readonly FacadeApiConfig _facadeApiConfig;

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
        _scopes = new[] {_facadeApiConfig.DownstreamScope};
    }

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
            ["currentPage"] = currentPage.ToString(), ["pageSize"] = _paginationConfig.PageSize.ToString(),
        };
        if (!string.IsNullOrEmpty(organisationName))
        {
            query["organisationName"] = organisationName;
        }

        if (!string.IsNullOrEmpty(applicationType))
        {
            query["applicationType"] = applicationType;
        }

        var queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var path = $"{_facadeApiConfig.Endpoints[PendingApplicationsEndpointKey]}?{queryString}";

        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedList<OrganisationApplications>>();
    }

    public async Task<OrganisationEnrolments> GetOrganisationEnrolments(Guid organisationId)
    {
        await PrepareAuthenticatedClient();

        var path = _facadeApiConfig.Endpoints[OrganisationEnrolmentsPath]
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
        return JsonSerializer.Deserialize<RegulatorCompanyDetailsModel>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EndpointResponseStatus> TransferOrganisationNation(
        OrganisationTransferNationRequest organisationNationTransfer)
    {
        await PrepareAuthenticatedClient();
        string path = _facadeApiConfig.Endpoints[OrganisationTransferNationPath];
        var response = await _httpClient.PostAsJsonAsync(path, organisationNationTransfer);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return EndpointResponseStatus.Success;
        }

        throw (new Exception(response.Content.ToString()));
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

    public async Task<PaginatedList<Submission>> GetOrganisationSubmissions(
        string? organisationName,
        string? organisationReference,
        string? organisationType,
        string[]? status,
        int currentPage = 1)
    {
        await PrepareAuthenticatedClient();

        var query = new Dictionary<string, string>
        {
            ["pageNumber"] = currentPage.ToString(),
            ["pageSize"] = _paginationConfig.PageSize.ToString(),
        };
        if (!string.IsNullOrEmpty(organisationName))
        {
            query["organisationName"] = organisationName;
        }

        if (!string.IsNullOrEmpty(organisationReference))
        {
            query["organisationReference"] = organisationReference;
        }

        if (!string.IsNullOrEmpty(organisationType))
        {
            query["organisationType"] = organisationType;
        }

        if (status != null)
        {
            query["Statuses"] = string.Join(',', status);
        }

        var queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var path = $"{_facadeApiConfig.Endpoints[PomSubmissionsEndpointKey]}?{queryString}";

        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedList<Submission>>();
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

        var path = string.Format(_facadeApiConfig.Endpoints[OrganisationsSearchPath], currentPage, _paginationConfig.PageSize, searchTerm);
        var response = await _httpClient.GetAsync(path);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaginatedList<OrganisationSearchResult>>();
    }

    public async Task<List<OrganisationUser>> GetProducerOrganisationUsersByOrganisationExternalId(Guid externalId)
    {
        await PrepareAuthenticatedClient();

        var path = string.Format(_facadeApiConfig.Endpoints[GetOrganisationUsersByOrganisationExternalIdPath], externalId);

        var response = await _httpClient.GetAsync(path);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<OrganisationUser>>();
    }

    public async Task<EndpointResponseStatus> RemoveApprovedUser(RemoveApprovedUserRequest request)
    {
        await PrepareAuthenticatedClient();
        var path = string.Format(_facadeApiConfig.Endpoints[OrganisationsRemoveApprovedUserPath], request.ConnectionExternalId, request.OrganisationId);

        var response = await _httpClient.PostAsJsonAsync(path, request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }
    public Task<EndpointResponseStatus> RejectSubmission() => throw new NotImplementedException();

    public Task<EndpointResponseStatus> AcceptSubmission() => throw new NotImplementedException();

    public Task<PaginatedList<Registration>> GetRegulatorRegistrations(
        string? organisationName,
        string? organisationReference,
        OrganisationType? organisationType,
        string[]? status,
        int currentPage = 1) =>
        throw new NotImplementedException();

    private async Task PrepareAuthenticatedClient()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_facadeApiConfig.BaseUrl);
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(Constants.Bearer, accessToken);
        }
    }
}