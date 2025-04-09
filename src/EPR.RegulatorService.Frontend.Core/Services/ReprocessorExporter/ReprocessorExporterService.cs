using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using Microsoft.Identity.Web;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

public class ReprocessorExporterService(
    HttpClient httpClient,
    ITokenAcquisition tokenAcquisition,
    IOptions<ReprocessorExporterFacadeApiConfig> reprocessorExporterFacadeConfig) : IReprocessorExporterService
{
    private enum Endpoints
    {
        GetRegistrationById,
        GetRegistrationMaterialById,
        UpdateRegistrationMaterialOutcome,
        UpdateRegistrationTaskStatus,
        UpdateApplicationTaskStatus
    }

    public async Task<Registration> GetRegistrationByIdAsync(int registrationId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationById);
        string path = pathTemplate.Replace("{id}", registrationId.ToString(CultureInfo.InvariantCulture));
        
        var response = await httpClient.GetAsync(path);
        
        var registration = await GetEntityFromResponse<Registration>(response);

        return registration;
    }

    public async Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(int registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationMaterialById);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationMaterialDetail = await GetEntityFromResponse<RegistrationMaterialDetail>(response);
        
        return registrationMaterialDetail;
    }

    public async Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.UpdateRegistrationMaterialOutcome);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.PatchAsJsonAsync(path, registrationMaterialOutcomeRequest);

        response.EnsureSuccessStatusCode();
    }

    private async Task PrepareAuthenticatedClient()
    {
        if (httpClient.BaseAddress == null)
        {
            httpClient.BaseAddress = new Uri(reprocessorExporterFacadeConfig.Value.BaseUrl);

            string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync([reprocessorExporterFacadeConfig.Value.DownstreamScope]);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Bearer, accessToken);
        }
    }

    private static async Task<T> GetEntityFromResponse<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var entity = await response.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        });

        if (entity == null)
        {
            throw new NotFoundException("Unable to deserialize response.");
        }

        return entity;
    }

    private string GetVersionedEndpoint(Endpoints endpointName)
    {
        string version = reprocessorExporterFacadeConfig.Value.ApiVersion.ToString();
        string pathTemplate = reprocessorExporterFacadeConfig.Value.Endpoints[endpointName.ToString()].Replace("{apiVersion}", version);

        return pathTemplate;
    }
}