namespace EPR.RegulatorService.Frontend.Core.Configs;

public class LandingPageConfig
{
    public const string ConfigSection = "LandingPageUrls";
    public string ManageAccountUrl { get; set; } = string.Empty;
    public string ApplicationsUrl { get; set; } = string.Empty;
    public string SubmissionsUrl { get; set; } = string.Empty;
    public string RegistrationsUrl { get; set; } = string.Empty;
    public string ManageApprovedPersonUrl { get; set; } = string.Empty;
}