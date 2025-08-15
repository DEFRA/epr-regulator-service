using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Http.Features;
using static System.Net.Mime.MediaTypeNames;
using System.Configuration;

namespace EPR.RegulatorService.Frontend.Web.Middleware;

public class JourneyAccessCheckerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JourneyAccessCheckerMiddleware(RequestDelegate next,IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(
        HttpContext httpContext,
        ISessionManager<JourneySession> sessionManager)
    {
        var healthCheckPath = _configuration.GetValue<string>("HealthCheckPath");
        if (!string.IsNullOrWhiteSpace(healthCheckPath) &&
            httpContext.Request.Path.StartsWithSegments(healthCheckPath, StringComparison.OrdinalIgnoreCase))
        {
            await _next(httpContext);
            return;
        }

        var endpoint = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<JourneyAccessAttribute>();

        if (attribute != null)
        {
            string? pageToRedirect = null;

            if (attribute.JourneyType == JourneyName.ManagePermissionsStart || attribute.JourneyType == JourneyName.ManagePermissions)
            {
                var id = ParseRouteId(httpContext);

                if (id == null)
                {
                    pageToRedirect = PagePath.Applications;
                }

                if (attribute.JourneyType == JourneyName.ManagePermissions)
                {
                    pageToRedirect = await InitialiseManagePermissionsJourney(httpContext, sessionManager, attribute, pageToRedirect, id);
                }
            }
            else
            {
                pageToRedirect = await InitializeApplicationsJourney(httpContext, sessionManager, attribute, pageToRedirect);
            }

            if (!string.IsNullOrEmpty(pageToRedirect))
            {
                httpContext.Response.Redirect($"{httpContext.Request.PathBase}/{pageToRedirect}");
                return;
            }
        }

        await _next(httpContext);

    }

    private static Guid? ParseRouteId(HttpContext context)
    {
        var id = context.GetRouteData().Values["id"];

        if (!Guid.TryParse(id?.ToString(), out var identifier))
        {
            return null;
        }

        return identifier;
    }

    private static async Task<string?> InitialiseManagePermissionsJourney(HttpContext httpContext, ISessionManager<JourneySession> sessionManager,
        JourneyAccessAttribute? attribute, string? pageToRedirect, Guid? id)
    {
        var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);
        var permissionManagementSession = sessionValue?.PermissionManagementSession;

        var permissionManagementSessionItem = permissionManagementSession?.Items.Find(x => x.Id == id);

        if (permissionManagementSessionItem == null || permissionManagementSessionItem.Journey.Count == 0)
        {
            pageToRedirect = PagePath.Applications;
        }
        else if (!permissionManagementSessionItem.Journey.Contains($"{attribute.PagePath}/{id}"))
        {
            var journeyList = permissionManagementSessionItem.Journey;
            pageToRedirect = journeyList[^1];
        }

        return pageToRedirect;
    }

    private static async Task<string?> InitializeApplicationsJourney(HttpContext httpContext, ISessionManager<JourneySession> sessionManager,
        JourneyAccessAttribute? attribute, string? pageToRedirect)
    {
        var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);
        var accountManagementSessionValue = sessionValue?.RegulatorSession;

        if (accountManagementSessionValue == null || accountManagementSessionValue.Journey.Count == 0)
        {
            pageToRedirect = PagePath.Applications;
        }
        else if (!accountManagementSessionValue.Journey.Contains(attribute.PagePath))
        {
            var journeyList = accountManagementSessionValue.Journey;
            pageToRedirect = journeyList[^1];
        }

        return pageToRedirect;
    }
}