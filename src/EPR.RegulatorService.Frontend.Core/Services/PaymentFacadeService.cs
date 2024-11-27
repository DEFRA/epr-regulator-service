using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.RegulatorService.Frontend.Core.Services;

public class PaymentFacadeService : IPaymentFacadeService
{
    private const string SubmitOfflinePaymentPath = "SubmitOfflinePaymentPath";
    private const string GetProducerPaymentDetailsPath = "GetProducerPaymentDetailsPath";
    private const string GetCompliancePaymentDetailsPath = "GetCompliancePaymentDetailsPath";

    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string[] _scopes;

    private readonly PaymentFacadeApiConfig _paymentFacadeApiConfig;

    public PaymentFacadeService(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IOptions<PaymentFacadeApiConfig> paymentFacadeApiOptions)
    {
        _httpClient = httpClient;
        _paymentFacadeApiConfig = paymentFacadeApiOptions.Value;
        _tokenAcquisition = tokenAcquisition;
        _scopes = [_paymentFacadeApiConfig.DownstreamScope];
    }

    public async Task<EndpointResponseStatus> SubmitOfflinePaymentAsync(OfflinePaymentRequest request)
    {
        await SetAuthorisationHeaderAsync();

        var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[SubmitOfflinePaymentPath], request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }

    public async Task<ProducerPaymentResponse?> GetProducerPaymentDetailsAsync(ProducerPaymentRequest request)
    {
        await SetAuthorisationHeaderAsync();

        var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[GetProducerPaymentDetailsPath], request);
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<ProducerPaymentResponse>(await response.Content.ReadAsStringAsync());
    }

    public async Task<CompliancePaymentResponse?> GetCompliancePaymentDetailsAsync(CompliancePaymentRequest request)
    {
        await SetAuthorisationHeaderAsync();

        var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[GetCompliancePaymentDetailsPath], request);
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<CompliancePaymentResponse>(await response.Content.ReadAsStringAsync());
    }

    private async Task SetAuthorisationHeaderAsync()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_paymentFacadeApiConfig.BaseUrl);
        }

        string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Bearer, accessToken);
    }
}