using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Configs;

[ExcludeFromCodeCoverage]
public class EprCookieOptions
{
   
    public const string ConfigSection = "CookieOptions";

    public const string B2CCookieName = "x-ms-cpim-*";
    public const string SessionCookieName = ".epr_session";
    public const string CookiePolicyCookieName = ".epr_regulator_cookies_policy";
    public const string AntiForgeryCookieName = ".epr_anti_forgery";
    public const string TsCookieName = "TSxxxxxxxx";
    public const string AuthenticationCookieName = ".epr_auth";
    public const string TempDataCookie = ".epr_temp";

    public int AuthenticationExpiryInMinutes { get; set; }

    public int CookiePolicyDurationInMonths { get; set; }
}