namespace EPR.RegulatorService.Frontend.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime UtcToGmt(this DateTime dateTime) =>
            TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/London"));

        public static string ToDisplayDate(this DateTime? dateTime) =>
            !dateTime.HasValue ? string.Empty : dateTime.Value.ToString("d MMMM yyyy");

        public static string ToDisplayDateAndTime(this DateTime? dateTime) =>
            !dateTime.HasValue
                ? string.Empty
                : dateTime.Value.ToDisplayDateAndTime();

        public static string ToDisplayDateAndTime(this DateTime dateTime) =>
            $"{dateTime:d MMMM yyyy} at {dateTime.ToString("h:mmtt").ToLower()}";
    }
}