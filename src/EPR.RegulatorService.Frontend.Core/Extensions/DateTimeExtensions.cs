namespace EPR.RegulatorService.Frontend.Core.Extensions;


public static class DateTimeExtensions
{
    public static DateTime UtcToGmt(this DateTime dateTime)
    {
        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/London"));
    }
}