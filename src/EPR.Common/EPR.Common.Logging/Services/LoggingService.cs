namespace EPR.Common.Logging.Services;

using Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Models;

internal class LoggingService : ILoggingService
{
    private readonly ILoggingApiClient _loggingApiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(
        ILoggingApiClient loggingApiClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LoggingService> logger)
    {
        _loggingApiClient = loggingApiClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendEventAsync(ProtectiveMonitoringEvent protectiveMonitoringEvent) =>
        await SendEventAsync(GetUserId(), protectiveMonitoringEvent);

    public async Task<HttpResponseMessage> SendEventAsync(
        Guid userId,
        ProtectiveMonitoringEvent protectiveMonitoringEvent)
    {
        LoggingEvent loggingEvent = new(
            userId,
            protectiveMonitoringEvent.SessionId,
            DateTime.UtcNow,
            protectiveMonitoringEvent.Component,
            protectiveMonitoringEvent.PmcCode,
            protectiveMonitoringEvent.Priority,
            new(
                protectiveMonitoringEvent.TransactionCode,
                protectiveMonitoringEvent.Message,
                protectiveMonitoringEvent.AdditionalInfo),
            GetIp());

        try
        {
            var response = await _loggingApiClient.SendEventAsync(loggingEvent);

            _logger.LogInformation("Logging event sent by {Component} in session {Session} for user {User} from {Ip} at {Time}",
                loggingEvent.Component, loggingEvent.SessionId,
                loggingEvent.UserId, loggingEvent.Ip, loggingEvent.DateTime);

            return response;
        }
        catch (HttpRequestException e)
        {
            var msg = $"Error sending logging event originated by {loggingEvent.Component} in session " +
                      $"{loggingEvent.SessionId} for user {loggingEvent.UserId} from {loggingEvent.Ip} at {loggingEvent.DateTime}";
            _logger.LogError(e, msg);
            throw;
        }
    }

    private string GetIp()
    {
        try
        {
            var headerNames = new[]
            {
                "HTTP_X_FORWARDED_FOR",
                "X-Forwarded-For",
                "REMOTE_ADDR",
                "HTTP_CLIENT_IP"
            };

            foreach (var headerName in headerNames)
            {
                if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(headerName, out var headerValue))
                {
                    var ipHeader = headerValue.FirstOrDefault();
                    if (!string.IsNullOrEmpty(ipHeader))
                    {
                        return ipHeader.Split(',').FirstOrDefault();
                    }
                }
            }

            return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        catch
        {
            return "0.0.0.0";
        }
    }

    private Guid GetUserId()
    {
        var user = _httpContextAccessor.HttpContext.User;
        var objectId = user.Claims.Single(claim => claim.Type == ClaimConstants.ObjectId);
        
        if (Guid.TryParse(objectId.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}
