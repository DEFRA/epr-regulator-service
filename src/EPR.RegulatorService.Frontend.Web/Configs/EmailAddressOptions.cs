using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public class EmailAddressOptions
{
    public const string ConfigSection = "EmailAddresses";

    public string DataProtection { get; set; }

    public string DefraGroupProtectionOfficer { get; set; }

    public string InformationCommissioner { get; set; }

    public string DefraHelpline { get; set; }
}
