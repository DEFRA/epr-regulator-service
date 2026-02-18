namespace EPR.Common.Logging.Clients;

using Models;

internal interface ILoggingApiClient
{
    Task<HttpResponseMessage> SendEventAsync(LoggingEvent loggingEvent);
}