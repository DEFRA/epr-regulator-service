namespace EPR.Common.Authorization.Config;

public class AzureB2CExtensionConfig
{
    public const string SectionName = "AzureAdB2C";

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string ExtensionsClientId { get; set; } = string.Empty;
}