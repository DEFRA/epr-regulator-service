namespace EPR.Common.Logging.Clients;

using System.Net.Http.Json;
using Exceptions;
using Models;

internal class LoggingApiClient : ILoggingApiClient
{
    private readonly HttpClient _httpClient;

    public LoggingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> SendEventAsync(LoggingEvent loggingEvent)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("log-events", loggingEvent);
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (Exception exception)
        {
            throw new ProtectiveMonitoringLogException(
                "Logging a protective monitoring event failed", exception);
        }
    }
}