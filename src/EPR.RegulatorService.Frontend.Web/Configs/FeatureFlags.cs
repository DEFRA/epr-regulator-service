using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public static class FeatureFlags
{
    public const string ShowLanguageSwitcher = "ShowLanguageSwitcher";

    public const string ManagePoMSubmissions = "ManagePoMSubmissions";

    public const string ManageRegistrations = "ManageRegistrations";

    public const string ManageApprovedUsers = "ManageApprovedUsers";

    public const string PomDataPeriodAndTime = "PomDataPeriodAndTime";

    public const string RegistrationDataPeriodAndTime = "RegistrationDataPeriodAndTime";

    public const string ManageRegistrationSubmissions = "ManageRegistrationSubmissions";
}