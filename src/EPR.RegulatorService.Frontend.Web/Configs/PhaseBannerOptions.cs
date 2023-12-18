namespace EPR.RegulatorService.Frontend.Web.Configs;

public class PhaseBannerOptions
{
    public const string ConfigSection = "PhaseBannerOptions";

    public string ApplicationStatus { get; set; } = string.Empty;

    public string SurveyUrl { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}