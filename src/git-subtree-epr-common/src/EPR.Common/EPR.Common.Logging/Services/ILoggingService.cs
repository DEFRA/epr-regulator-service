namespace EPR.Common.Logging.Services;

using Models;

public interface ILoggingService
{
   Task<HttpResponseMessage> SendEventAsync(ProtectiveMonitoringEvent protectiveMonitoringEvent);

   Task<HttpResponseMessage> SendEventAsync(Guid userId, ProtectiveMonitoringEvent protectiveMonitoringEvent);
}