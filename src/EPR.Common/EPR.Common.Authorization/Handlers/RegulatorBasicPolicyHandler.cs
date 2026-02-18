using System.Security.Claims;
using EPR.Common.Authorization.Config;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Helpers;
using EPR.Common.Authorization.Interfaces;
using EPR.Common.Authorization.Requirements;
using EPR.Common.Authorization.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Common.Authorization.Handlers;

public sealed class RegulatorBasicPolicyHandler<TSessionType>
    : PolicyHandlerBase<RegulatorBasicPolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    private readonly ILogger<RegulatorBasicPolicyHandler<TSessionType>> _logger;

    public RegulatorBasicPolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<RegulatorBasicPolicyHandler<TSessionType>> logger)
        : base(sessionManager, httpClientFactory, options, logger)
    {
        _logger = logger;
    }

    protected override string PolicyHandlerName => nameof(RegulatorBasicPolicyHandler<TSessionType>);
    protected override string PolicyDescription => ServiceRoles.RegulatorBasic;

    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
      ClaimsPrincipleHelper.IsRegulator;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RegulatorBasicPolicyRequirement requirement)
    {
        var httpContext =
            context.Resource as HttpContext ??
            (context.Resource as AuthorizationFilterContext)?.HttpContext;

        // 1) If endpoint allows anonymous, do nothing (don’t hit base).
        if (httpContext?.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return;


        // 2) If the user is unauthenticated and the endpoint requires auth,
        //    let the framework challenge — do NOT call base (avoids the warning in base).
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("{Policy} unauthenticated request to {Path}",
                PolicyHandlerName, httpContext?.Request.Path.Value ?? string.Empty);
            return;

        }

        // 3) Authenticated: proceed with base
        await base.HandleRequirementAsync(context, requirement);
    }
}
