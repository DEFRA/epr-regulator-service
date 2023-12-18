using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Cookies;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.Middleware;

[ExcludeFromCodeCoverage]
public class AnalyticsCookieMiddleware
{
    private readonly RequestDelegate _next;

    public AnalyticsCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        ICookieService cookieService,
        IOptions<AnalyticsOptions> googleAnalyticsOptions)
    {
        httpContext.Items[ContextKeys.UseGoogleAnalyticsCookieKey] = cookieService.HasUserAcceptedCookies(httpContext.Request.Cookies);
        httpContext.Items[ContextKeys.TagManagerContainerIdKey] = googleAnalyticsOptions.Value.TagManagerContainerId;

        await _next(httpContext);
    }
}
