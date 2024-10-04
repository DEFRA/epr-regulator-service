using System.Net;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController : Controller
{
    [ActionName(PagePath.Error)]
    [Route(PagePath.Error)]
    [Route(PagePath.PageNotFoundPath)]
    public ViewResult Error(int? statusCode, string? backLink)
    {
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
}
