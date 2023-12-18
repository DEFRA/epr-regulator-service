using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public class ServiceSettingsOptions
{
    public const string ConfigSection = "ServiceSettings";

    public string ServiceKey { get; set; } = string.Empty;
}