using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Converters;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using Enums;

public class ReprocessorExporterService(
    HttpClient httpClient,
    ITokenAcquisition tokenAcquisition,
    IOptions<ReprocessorExporterFacadeApiConfig> reprocessorExporterFacadeConfig) : IReprocessorExporterService
{
    private enum Endpoints
    {
        AddMaterialQueryNote,
        AddRegistrationQueryNote,
        GetRegistrationById,
        GetRegistrationMaterialById,
        GetAuthorisedMaterialsByRegistrationId,
        GetWasteCarrierDetailsByRegistrationId,
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
        DownloadSamplingInspectionFile,
        GetRegistrationByIdWithAccreditations,
        GetPaymentFeesByAccreditationId,
        SubmitAccreditationOfflinePayment,
        MarkAccreditationAsDulyMade,
        UpdateAccreditationTaskStatus,
        GetSamplingPlanByAccreditationId,
        DownloadAccreditationSamplingInspectionFile
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

    public async Task<Registration> GetRegistrationByIdAsync(Guid id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationById);
        string path = pathTemplate.Replace("{id}", id.ToString());

        var response = await httpClient.GetAsync(path);

        var registration = await GetEntityFromResponse<Registration>(response);

        return registration;
    }

    public async Task<SiteDetails> GetSiteDetailsByRegistrationIdAsync(Guid id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetSiteAddressByRegistrationId);
        string path = pathTemplate.Replace("{id}", id.ToString());

        var response = await httpClient.GetAsync(path);

        var siteDetails = await GetEntityFromResponse<SiteDetails>(response);

        return siteDetails;
    }

    public async Task<RegistrationMaterialDetail> GetRegistrationMaterialByIdAsync(Guid id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationMaterialById);
        string path = pathTemplate.Replace("{id}", id.ToString());

        var response = await httpClient.GetAsync(path);

        var registrationMaterialDetail = await GetEntityFromResponse<RegistrationMaterialDetail>(response);

        return registrationMaterialDetail;
    }

    public async Task<RegistrationAuthorisedMaterials> GetAuthorisedMaterialsByRegistrationIdAsync(Guid registrationId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetAuthorisedMaterialsByRegistrationId);
        string path = pathTemplate.Replace("{id}", registrationId.ToString());

        var response = await httpClient.GetAsync(path);

        var registrationAuthorisedMaterials = await GetEntityFromResponse<RegistrationAuthorisedMaterials>(response);

        return registrationAuthorisedMaterials;
    }

    public async Task<RegistrationMaterialWasteLicence> GetWasteLicenceByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetWasteLicenceByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString());

        var response = await httpClient.GetAsync(path);

        var registrationMaterialWasteLicence = await GetEntityFromResponse<RegistrationMaterialWasteLicence>(response);

        return registrationMaterialWasteLicence;
    }

    public async Task<RegistrationMaterialPaymentFees> GetPaymentFeesByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetPaymentFeesByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString());

        var response = await httpClient.GetAsync(path);

        var registrationMaterialPaymentFees = await GetEntityFromResponse<RegistrationMaterialPaymentFees>(response);

        return registrationMaterialPaymentFees;
    }

    public async Task<WasteCarrierDetails> GetWasteCarrierDetailsByRegistrationIdAsync(Guid registrationId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetWasteCarrierDetailsByRegistrationId);
        string path = pathTemplate.Replace("{id}", registrationId.ToString());

        var response = await httpClient.GetAsync(path);

        var wasteCarrierDetails = await GetEntityFromResponse<WasteCarrierDetails>(response);

        return wasteCarrierDetails;
    }

    public async Task MarkAsDulyMadeAsync(Guid registrationMaterialId, MarkAsDulyMadeRequest dulyMadeRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.MarkAsDulyMade);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString());

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

    public async Task UpdateRegistrationMaterialOutcomeAsync(Guid registrationMaterialId, RegistrationMaterialOutcomeRequest registrationMaterialOutcomeRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.UpdateRegistrationMaterialOutcome);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString());

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

    public async Task<RegistrationMaterialReprocessingIO> GetReprocessingIOByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetReprocessingIOByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString());

        var response = await httpClient.GetAsync(path);

        var registrationMaterialReprocessingIO = await GetEntityFromResponse<RegistrationMaterialReprocessingIO>(response);

        return registrationMaterialReprocessingIO;
    }

    public async Task<RegistrationMaterialSamplingPlan> GetSamplingPlanByRegistrationMaterialIdAsync(Guid registrationMaterialId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetSamplingPlanByRegistrationMaterialId);
        string path = pathTemplate.Replace("{id}", registrationMaterialId.ToString());

        var response = await httpClient.GetAsync(path);

        var registrationMaterialSamplingPlan = await GetEntityFromResponse<RegistrationMaterialSamplingPlan>(response);

        return registrationMaterialSamplingPlan;
    }

    public async Task<HttpResponseMessage> DownloadSamplingInspectionFile(FileDownloadRequest request)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate;
        switch (request.SubmissionType)
        {
            case SubmissionType.Accreditation:
                pathTemplate = GetVersionedEndpoint(Endpoints.DownloadAccreditationSamplingInspectionFile);
                break;
            default:
                pathTemplate = GetVersionedEndpoint(Endpoints.DownloadSamplingInspectionFile);
                break;
        }

        var response = await httpClient.PostAsJsonAsync(pathTemplate, request);

        if (!response.IsSuccessStatusCode)
        {
            throw new NotFoundException("Unable to download file.");
        }
        return response;
    }

    public async Task<Registration> GetRegistrationByIdWithAccreditationsAsync(Guid id, int? year = null)
    {
        await PrepareAuthenticatedClient();

        // Backend may already filter by year (if supported), so we pass it along
        var registration = await GetAccreditationRegistrationAsync(id, year);

        // Defensive fallback: in case backend ignores or misapplies year filter
        if (year.HasValue)
        {
            ApplySingleYearAccreditationFilter(registration, year.Value);
        }

        return registration;
    }

    private static void ApplySingleYearAccreditationFilter(Registration registration, int year)
    {
        bool hasAtLeastOneAccreditation = false;

        foreach (var material in registration.Materials)
        {
            var matchingAccreditations = material.Accreditations
                .Where(a => a.AccreditationYear == year)
                .ToList();

            if (matchingAccreditations.Count > 1)
            {
                throw new InvalidOperationException(
                    $"More than one accreditation found for MaterialId {material.Id} in year {year}.");
            }

            if (matchingAccreditations.Count == 1)
            {
                hasAtLeastOneAccreditation = true;
                material.Accreditations = matchingAccreditations;
            }
            else
            {
                // No matching accreditations � clear out the list for cleanliness
                material.Accreditations = new List<Accreditation>();
            }
        }

        if (!hasAtLeastOneAccreditation)
        {
            throw new InvalidOperationException(
                $"No accreditations found for any materials in year {year}.");
        }
    }
    
    private async Task<Registration> GetAccreditationRegistrationAsync(Guid id, int? year)
    {
        string pathTemplate = GetVersionedEndpoint(Endpoints.GetRegistrationByIdWithAccreditations);
        string path = pathTemplate.Replace("{id}", id.ToString());

        if (year.HasValue)
        {
            path += $"?year={year.Value}";
        }

        var response = await httpClient.GetAsync(path);
        return await GetEntityFromResponse<Registration>(response);
    }

    public async Task<AccreditationMaterialPaymentFees> GetPaymentFeesByAccreditationIdAsync(Guid id)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetPaymentFeesByAccreditationId);
        string path = pathTemplate.Replace("{id}", id.ToString());

        var response = await httpClient.GetAsync(path);

        var accreditationMaterialPaymentFees = await GetEntityFromResponse<AccreditationMaterialPaymentFees>(response);

        return accreditationMaterialPaymentFees;
    }

    public async Task SubmitAccreditationOfflinePaymentAsync(AccreditationOfflinePaymentRequest offlinePayment)
    {
        await PrepareAuthenticatedClient();

        string path = GetVersionedEndpoint(Endpoints.SubmitAccreditationOfflinePayment);

        string jsonContent = JsonSerializer.Serialize(offlinePayment, _jsonSerializerOptions);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(path, content);

        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAccreditationAsDulyMadeAsync(Guid accreditationId, AccreditationMarkAsDulyMadeRequest dulyMadeRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.MarkAccreditationAsDulyMade);
        string path = pathTemplate.Replace("{id}", accreditationId.ToString());

        string jsonContent = JsonSerializer.Serialize(dulyMadeRequest, _jsonSerializerOptions);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(path, content);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRegulatorAccreditationTaskStatusAsync(UpdateAccreditationTaskStatusRequest updateAccreditationTaskStatusRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.UpdateAccreditationTaskStatus);

        var response = await httpClient.PostAsJsonAsync(pathTemplate, updateAccreditationTaskStatusRequest);

        response.EnsureSuccessStatusCode();
    }

    public async Task AddMaterialQueryNoteAsync(Guid regulatorApplicationTaskStatusId, AddNoteRequest addNoteRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.AddMaterialQueryNote);
        string path = pathTemplate.Replace("{id}", regulatorApplicationTaskStatusId.ToString());

        var response = await httpClient.PostAsJsonAsync(path, addNoteRequest);

        response.EnsureSuccessStatusCode();
    }

    public async Task AddRegistrationQueryNoteAsync(Guid regulatorRegistrationTaskStatusId, AddNoteRequest addNoteRequest)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.AddRegistrationQueryNote);
        string path = pathTemplate.Replace("{id}", regulatorRegistrationTaskStatusId.ToString());

        var response = await httpClient.PostAsJsonAsync(path, addNoteRequest);

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

    public async Task<AccreditationSamplingPlan> GetSamplingPlanByAccreditationIdAsync(Guid accreditationId)
    {
        await PrepareAuthenticatedClient();

        string pathTemplate = GetVersionedEndpoint(Endpoints.GetSamplingPlanByAccreditationId);
        string path = pathTemplate.Replace("{id}", accreditationId.ToString());

        var response = await httpClient.GetAsync(path);
        var accreditationSamplingPlan = await GetEntityFromResponse<AccreditationSamplingPlan>(response);

        return accreditationSamplingPlan;
    }
}