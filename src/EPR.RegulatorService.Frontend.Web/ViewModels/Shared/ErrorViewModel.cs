namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared
{
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Web.Extensions;

    public class ErrorViewModel
    {
        public string Timestamp { get; set; } = DateTime.UtcNow.UtcToGmt().ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);
    }
}
