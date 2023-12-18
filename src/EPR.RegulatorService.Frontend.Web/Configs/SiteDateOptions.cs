using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public class SiteDateOptions
{
    public const string ConfigSection = "SiteDates";

    public DateTime PrivacyLastUpdated { get; set; }

    public DateTime AccessibilityStatementPrepared { get; set; }

    public DateTime AccessibilityStatementReviewed { get; set; }

    public DateTime AccessibilitySiteTested { get; set; }

    public string DateFormat { get; set; }
}