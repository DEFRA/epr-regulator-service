using System.Net;

using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Errors;

[AllowAnonymous]
public partial class ErrorController(ILogger<ErrorController> logger) : Controller
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Unhandled Application Error: status code {StatusCode} backlink {Backlink}")]
    private static partial void LogUnhandledApplicationError(ILogger logger, int? statusCode, string? backLink);

    [ActionName(PagePath.Error)]
    [Route(PagePath.Error)]
    [Route(PagePath.PageNotFoundPath)]
    public ViewResult Error(int? statusCode, string? backLink)
    {
        LogUnhandledApplicationError(logger, statusCode, backLink);

        bool isPageNotFoundError = statusCode == (int?)HttpStatusCode.NotFound;
        string errorView = isPageNotFoundError ? PagePath.PageNotFound : "Error";

        // Status code of 200 required for Gateway
        Response.StatusCode = 200;

        if (isPageNotFoundError && !string.IsNullOrEmpty(backLink))
        {
            ViewBag.BackLinkToDisplay = backLink;
        }

        return View(errorView, new ErrorViewModel());
    }

    [Route(PagePath.ServiceNotAvailable, Name = "ServiceNotAvailable")]
    public ViewResult ServiceNotAvailable(string? backLink)
    {
        ViewBag.BackLinkToDisplay = backLink;
        return View();
    }
}