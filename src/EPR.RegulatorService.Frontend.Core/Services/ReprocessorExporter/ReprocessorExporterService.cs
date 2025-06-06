using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Converters;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using Microsoft.Identity.Web;
using Microsoft.Extensions.Options;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;

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
        GetAuthorisedMaterialsByRegistrationId,
        GetWasteLicenceByRegistrationMaterialId,
        GetPaymentFeesByRegistrationMaterialId,
        MarkAsDulyMade,
        SubmitOfflinePayment,
        UpdateRegistrationMaterialOutcome,
        UpdateRegistrationTaskStatus,
        GetReprocessingIOByRegistrationMaterialId,
        GetSamplingPlanByRegistrationMaterialId,
        UpdateApplicationTaskStatus,
        GetSiteAddressByRegistrationId,
        DownloadSamplingInspectionFile
    }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new PaymentMethodTypeConverter()
        }
    };

    public async Task<Registration> GetRegistrationByIdAsync(int id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationById);
        string path = pathTemplate.Replace("{id}", id.ToString(CultureInfo.InvariantCulture));
        
        var response = await httpClient.GetAsync(path);
        
        var registration = await GetEntityFromResponse<Registration>(response);

        return registration;
    }

    public async Task<SiteDetails> GetSiteDetailsByRegistrationIdAsync(int id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetSiteAddressByRegistrationId);
        string path = pathTemplate.Replace("{id}", id.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var siteDetails = await GetEntityFromResponse<SiteDetails>(response);

        return siteDetails;
    }

    public async Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(int id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationMaterialById);
        string path = pathTemplate.Replace("{id}", id.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationMaterialDetail = await GetEntityFromResponse<RegistrationMaterialDetail>(response);
        
        return registrationMaterialDetail;
    }

    public async Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(int registrationId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetAuthorisedMaterialsByRegistrationId);
        string path = pathTemplate.Replace("{id}", registrationId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationAuthorisedMaterials = await GetEntityFromResponse<RegistrationAuthorisedMaterials>(response);

        return registrationAuthorisedMaterials;
    }

    public async Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(int registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetWasteLicenceByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationMaterialWasteLicence = await GetEntityFromResponse<RegistrationMaterialWasteLicence>(response);

        return registrationMaterialWasteLicence;
    }

    public async Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(int registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetPaymentFeesByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationMaterialPaymentFees = await GetEntityFromResponse<RegistrationMaterialPaymentFees>(response);

        return registrationMaterialPaymentFees;
    }

    public async Task MarkAsDulyMadeAsync(int registrationMaterialId, MarkAsDulyMadeRequest dulyMadeRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.MarkAsDulyMade);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        string jsonContent = JsonSerializer.Serialize(dulyMadeRequest, _jsonSerializerOptions);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(path, content);

        response.EnsureSuccessStatusCode();
    }

    public async Task SubmitOfflinePaymentAsync(OfflinePaymentRequest offlinePayment)
    {
        await PrepareAuthenticatedClient();

        string path = GetVersionedEndpoint(Endpoints.SubmitOfflinePayment);
        
        string jsonContent = JsonSerializer.Serialize(offlinePayment, _jsonSerializerOptions);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(path, content);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRegistrationMaterialOutcomeAsync(int registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.UpdateRegistrationMaterialOutcome);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        string jsonContent = JsonSerializer.Serialize(registrationMaterialOutcomeRequest, _jsonSerializerOptions);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(path, content);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRegulatorRegistrationTaskStatusAsync(UpdateRegistrationTaskStatusRequest updateRegistrationTaskStatusRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.UpdateRegistrationTaskStatus);        

        var response = await httpClient.PostAsJsonAsync(pathTemplate, updateRegistrationTaskStatusRequest);

        response.EnsureSuccessStatusCode();
    }
    public async Task UpdateRegulatorApplicationTaskStatusAsync(UpdateMaterialTaskStatusRequest updateMaterialTaskStatusRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.UpdateApplicationTaskStatus);

        var response = await httpClient.PostAsJsonAsync(pathTemplate, updateMaterialTaskStatusRequest);

        response.EnsureSuccessStatusCode();
    }

    public async Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(int registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetReprocessingIOByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationMaterialReprocessingIO = await GetEntityFromResponse<RegistrationMaterialReprocessingIO>(response);

        return registrationMaterialReprocessingIO;
    }

    public async Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(int registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetSamplingPlanByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString(CultureInfo.InvariantCulture));

        var response = await httpClient.GetAsync(path);

        var registrationMaterialSamplingPlan = await GetEntityFromResponse<RegistrationMaterialSamplingPlan>(response);

        return registrationMaterialSamplingPlan;
    }

    public async Task<HttpResponseMessage> DownloadSamplingInspectionFile(FileDownloadRequest request)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.DownloadSamplingInspectionFile);

        var response = await httpClient.PostAsJsonAsync(pathTemplate, request);

        if (!response.IsSuccessStatusCode)
        {
            throw new NotFoundException("Unable to download file.");
        }
        return response;
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

    private async Task<T> GetEntityFromResponse<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var entity = await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions);

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