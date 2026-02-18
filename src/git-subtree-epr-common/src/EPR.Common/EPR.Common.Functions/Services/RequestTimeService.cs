namespace EPR.Common.Functions.Services;

using Interfaces;

public class RequestTimeService : IRequestTimeService
{
    public RequestTimeService(ITimeService timeService) => this.UtcRequest = timeService.UtcNow;

    public DateTime UtcRequest { get; }
}