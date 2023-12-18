using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Extensions;
[ExcludeFromCodeCoverage]
public static class UrlHelperExtension
{
    public static string HomePath(this IUrlHelper url)
    {
        return url.Action("Applications", "Applications");
    }
    
    public static string CurrentPath(this IUrlHelper url)
    {
        var routeValues = url.ActionContext.RouteData.Values;
        return url.Action(routeValues["action"]?.ToString(), routeValues["controller"]?.ToString());
    }
}