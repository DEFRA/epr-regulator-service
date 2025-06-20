namespace EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.Configs;
    using EPR.RegulatorService.Frontend.Core.DTOs;
    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;

    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Web;

    public class ManageRegistrationSubmissionsService(
        HttpClient httpClient,
        IOptions<FacadeApiConfig> facadeApiConfig,
        ITokenAcquisition tokenAcquisition) : IManageRegistrationSubmissionsService
    {
        private const string GetOrganisationRegistrationSubmissionDetailsPath = "GetOrganisationRegistrationSubmissionDetailsPath";

        private readonly string[] _scopes = [facadeApiConfig.Value.DownstreamScope];

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public Task<RegistrationSubmissionsDto> GetRegistrationSubmissionsAsync(int? pageNumber) => throw new NotImplementedException();

        public async Task<RegistrationSubmissionDetailsDto> GetRegistrationSubmissionDetailsAsync(Guid submissionId)
        {
            await PrepareAuthenticatedClient();

            string path = facadeApiConfig.Value.Endpoints[GetOrganisationRegistrationSubmissionDetailsPath].Replace("{0}", submissionId.ToString());

            var response = await httpClient.GetAsync(path);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            var facadeResponseDto = JsonSerializer.Deserialize<RegistrationSubmissionDetailsDto>(content, _jsonSerializerOptions);

            return facadeResponseDto;
        }

        private async Task PrepareAuthenticatedClient()
        {
            if (httpClient.BaseAddress == null)
            {
                httpClient.BaseAddress = new Uri(facadeApiConfig.Value.BaseUrl);
                string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(Constants.Bearer, accessToken);
            }
        }
    }
}
