namespace EPR.RegulatorService.Frontend.Web.Cookies;

public static partial class CookieServiceMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Error setting cookie acceptance to '{Accept}'")]
    public static partial void SettingCookieAcceptanceException(this ILogger logger, bool accept);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error reading cookie acceptance")]
    public static partial void ReadCookieAcceptanceException(this ILogger logger);
}