using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EPR.RegulatorService.Frontend.Web.Filters;

public class CheckSubmissionAndLoadIntoSessionFilter(ISessionManager<JourneySession> sessionManager, IFacadeService facadeService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (((ControllerActionDescriptor)context.ActionDescriptor).ActionName == PagePath.PageNotFound)
        {
            await next();
            return;
        }

        await FetchFromSessionOrFacadeAsync(context);
        await next();
    }

    private async Task<IActionResult> FetchFromSessionOrFacadeAsync(ActionExecutingContext context)
    {
        var currentSession = await sessionManager.GetSessionAsync(context.HttpContext.Session) ?? new JourneySession();
        var submissionId = context.ActionArguments.TryGetValue("submissionId", out object? value) ? (Guid)value : Guid.Empty;
        if (submissionId == Guid.Empty)
        {
            return default;
        }

        if (currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations.ContainsKey(submissionId))
        {
            return default;
        }

        var submission = await facadeService.GetRegistrationSubmissionDetails(submissionId);
        if (submission is not null)
        {
            currentSession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[submission.SubmissionId] = submission;
            return default;
        }

        return new RedirectToActionResult(PagePath.PageNotFound, "RegistrationSubmissions", default);
    }
}