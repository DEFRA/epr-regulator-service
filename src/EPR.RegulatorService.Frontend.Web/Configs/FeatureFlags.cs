using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public static class FeatureFlags
{
    public const string ShowLanguageSwitcher = "ShowLanguageSwitcher";

    public const string ManagePoMSubmissions = "ManagePoMSubmissions";

    public const string ManageApprovedUsers = "ManageApprovedUsers";
}