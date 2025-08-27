namespace EPR.RegulatorService.Frontend.Web.Extensions;

public static partial class LoggerMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "L2 Cache Failure Redis connection exception: {Message}")]
    public static partial void RedisConnectionException(this ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "L2 Cache Failure: {Message}")]
    public static partial void RedisCacheFailure(this ILogger logger, string message);
}