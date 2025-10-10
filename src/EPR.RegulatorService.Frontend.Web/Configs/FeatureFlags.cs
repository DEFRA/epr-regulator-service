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

    public const string ReprocessorExporter = "ReprocessorExporter";

    public const string EnableCsvDownload = "EnableCsvDownload";

    public const string ShowYourFeedbackFooter = "ShowYourFeedbackFooter";

    public const string ShowFeesWaiveButton = "ShowFeesWaiveButton";
    public const string ShowComplianceSchemeMembership = "ShowComplianceSchemeMembership";

    public const string IncludeSubsidiariesInFeeCalculationsForRegulators = "IncludeSubsidiariesInFeeCalculationsForRegulators";
}