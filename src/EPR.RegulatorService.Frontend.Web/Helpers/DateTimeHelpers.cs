namespace EPR.RegulatorService.Frontend.Web.Helpers;

using System.Globalization;

public static class DateTimeHelpers
{
    public static string FormatTimeAndDateForSubmission(DateTime timeAndDateOfSubmission)
    {
        string time = timeAndDateOfSubmission.ToString("h:mm", CultureInfo.CurrentCulture);
        string ampm = timeAndDateOfSubmission.ToString("tt", CultureInfo.CurrentCulture).ToLower(System.Globalization.CultureInfo.InvariantCulture);
        string date = timeAndDateOfSubmission.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
        return $"{time}{ampm} on {date}";
    }
}