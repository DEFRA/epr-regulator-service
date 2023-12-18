using Microsoft.AspNetCore.Localization;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Sessions;
[ExcludeFromCodeCoverage]
public class SessionRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        await Task.Yield();
        var culture = httpContext.Session.Get(EPR.RegulatorService.Frontend.Web.Constants.Language.SessionLanguageKey) == null ? EPR.RegulatorService.Frontend.Web.Constants.Language.English : httpContext.Session.GetString(EPR.RegulatorService.Frontend.Web.Constants.Language.SessionLanguageKey);
        return new ProviderCultureResult(culture);
    }
}