using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Diagnostics;

namespace EPR.RegulatorService.Frontend.Web.Middleware;

[ExcludeFromCodeCoverage]
public class BackLinkQuerystringAppenderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var feature = httpContext.Features.Get<IStatusCodeReExecuteFeature>();

        if (feature != null)
        {
            httpContext.Request.QueryString = new QueryString(
                httpContext.Request.QueryString + $"&backLink={feature.OriginalPathBase}{feature.OriginalPath}{feature.OriginalQueryString}"
            );
        }

        await next(httpContext);
    }
}