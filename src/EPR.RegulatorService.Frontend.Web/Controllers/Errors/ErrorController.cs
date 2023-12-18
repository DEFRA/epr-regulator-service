using System.Net;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController : Controller
{
    [ActionName(PagePath.Error)]
    [Route(PagePath.PageNotFoundPath)]
    public ViewResult Error(int? statusCode, string? backLink)
    {
        string errorView = statusCode == (int?)HttpStatusCode.NotFound ? PagePath.PageNotFound : "Error";

        // Status code of 200 required for Gateway
        Response.StatusCode = 200;

        ViewBag.BackLinkToDisplay = backLink ?? PagePath.Home;

        return View(errorView);
    }
}
