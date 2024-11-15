using System.Net.Http.Headers;
using System.Net.Http.Json;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.RegulatorService.Frontend.Core.Services;

public class PaymentFacadeService : IPaymentFacadeService
{
    private const string SubmitOfflinePaymentPath = "SubmitOfflinePaymentPath";

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
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_paymentFacadeApiConfig.BaseUrl);

            //To Do:: Update the downstream scope in the app settings once we have the correct one for payment facade api
            string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.Bearer, accessToken);
        }

        var response = await _httpClient.PostAsJsonAsync(_paymentFacadeApiConfig.Endpoints[SubmitOfflinePaymentPath], request);

        return response.IsSuccessStatusCode ? EndpointResponseStatus.Success : EndpointResponseStatus.Fail;
    }
}