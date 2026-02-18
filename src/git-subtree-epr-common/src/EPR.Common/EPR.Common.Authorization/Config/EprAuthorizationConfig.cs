namespace EPR.Common.Authorization.Config;

public class EprAuthorizationConfig
{
    public const string SectionName = "EprAuthorizationConfig";

    public string FacadeBaseUrl { get; set; } = string.Empty;

    public string FacadeUserAccountEndpoint { get; set; } = string.Empty;

    public string FacadeDownStreamScope { get; set; } = string.Empty;

    public string SignInRedirect { get; set; } = string.Empty;
}