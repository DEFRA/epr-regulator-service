namespace EPR.Common.Functions.Services;

using Interfaces;

public sealed class TimeService : ITimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}