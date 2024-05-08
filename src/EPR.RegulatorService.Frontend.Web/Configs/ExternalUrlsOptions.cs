using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public class ExternalUrlsOptions
{
    public const string ConfigSection = "ExternalUrls";

    public string GovUkHome { get; set; }

    public string PrivacyDefrasPersonalInformationCharter { get; set; }

    public string PrivacyInformationCommissioner { get; set; }

    public string AccessibilityAbilityNet { get; set; }

    public string AccessibilityEqualityAdvisorySupportService { get; set; }

    public string AccessibilityContactUs { get; set; }

    public string AccessibilityWebContentAccessibility { get; set; }

    public string LandingPageUrl { get; set; }

    public string PrivacyDataProtectionPublicRegister { get; set; }

    public string PrivacyScottishEnvironmentalProtectionAgency { get; set; }

    public string PrivacyNationalResourcesWales { get; set; }

    public string PrivacyNorthernIrelandEnvironmentAgency { get; set; }

    public string PrivacyEnvironmentAgency { get; set; }

    public string PowerBiLogin { get; set; }

    public string CompaniesHouseRegisterBaseUrl { get; set; }

    public string RegistrationFileDownloadBaseUrl { get; set; }
}
