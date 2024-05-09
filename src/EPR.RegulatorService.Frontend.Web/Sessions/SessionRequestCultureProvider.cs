using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Localization;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Sessions;
[ExcludeFromCodeCoverage]
public class SessionRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var culture = httpContext.Session.GetString(Language.SessionLanguageKey);

        if (culture == null && httpContext.Request.Path.Value == $"/{PagePath.SignedOut}")
        {
            var cultureFromQuery = httpContext.Request.Query["culture"].ToString();

            if (!string.IsNullOrEmpty(cultureFromQuery))
            {
                culture = cultureFromQuery;
            }
        }

        return await Task.FromResult(new ProviderCultureResult(culture ?? Language.English));
    }
}