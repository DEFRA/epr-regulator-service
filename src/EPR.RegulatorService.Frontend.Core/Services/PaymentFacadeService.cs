using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.RegulatorService.Frontend.Core.Services;

public class PaymentFacadeService : IPaymentFacadeService
{
    private const string SubmitOfflinePaymentPath = "SubmitOfflinePaymentPath";
    private const string GetProducerPaymentDetailsPath = "GetProducerPaymentDetailsPath";
    private const string GetCompliancePaymentDetailsPath = "GetCompliancePaymentDetailsPath";
    private const string GetProducerPaymentDetailsForResubmissionPath = "GetProducerPaymentDetailsForResubmissionPath";
    private const string GetCompliancePaymentDetailsResubmissionPath = "GetCompliancePaymentDetailsResubmissionPath";

    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string[] _scopes;

    private readonly PaymentFacadeApiConfig _paymentFacadeApiConfig;
    private readonly ILogger<PaymentFacadeService> _logger;

    private static readonly Action<ILogger, string, Exception?> _logPaymentFacadeServiceError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1001, nameof(PaymentFacadeService)),
            "Exception: {ErrorMessage}");

    public PaymentFacadeService(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IOptions<PaymentFacadeApiConfig> paymentFacadeApiOptions,
        ILogger<PaymentFacadeService> logger)
    {
        _httpClient = httpClient;
        _paymentFacadeApiConfig = paymentFacadeApiOptions.Value;
        _tokenAcquisition = tokenAcquisition;
        _scopes = [_paymentFacadeApiConfig.DownstreamScope];
        _logger = logger;
    }

    public async Task<EndpointResponseStatus> SubmitOfflinePaymentAsync(OfflinePaymentRequest request)
    {
        try
        {
            await SetAuthorisationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[SubmitOfflinePaymentPath], request);
            response.EnsureSuccessStatusCode();
            return EndpointResponseStatus.Success;
        }
        catch (Exception ex)
        {
            _logPaymentFacadeServiceError.Invoke(_logger, $"Error occurred while submitting offline payment {nameof(PaymentFacadeService)}.{nameof(SubmitOfflinePaymentAsync)}", ex);
        }
        return EndpointResponseStatus.Fail;
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

    public async Task<PackagingProducerPaymentResponse?> GetProducerPaymentDetailsForResubmissionAsync(PackagingProducerPaymentRequest request)
    {
        await SetAuthorisationHeaderAsync();

        var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[GetProducerPaymentDetailsForResubmissionPath], request);
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PackagingProducerPaymentResponse>(await response.Content.ReadAsStringAsync());
    }

    public async Task<PackagingCompliancePaymentResponse?> GetCompliancePaymentDetailsForResubmissionAsync(PackagingCompliancePaymentRequest request)
    {
        await SetAuthorisationHeaderAsync();

        var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[GetCompliancePaymentDetailsResubmissionPath], request);
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PackagingCompliancePaymentResponse>(await response.Content.ReadAsStringAsync());
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