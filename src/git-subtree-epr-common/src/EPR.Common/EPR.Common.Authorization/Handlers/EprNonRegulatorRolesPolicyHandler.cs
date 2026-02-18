namespace EPR.Common.Authorization.Handlers;

using System.Security.Claims;
using Config;
using Helpers;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Requirements;
using Sessions;

public sealed class EprNonRegulatorRolesPolicyHandler<TSessionType>
    : PolicyHandlerBase<EprNonRegulatorRolesPolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    public EprNonRegulatorRolesPolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<EprNonRegulatorRolesPolicyHandler<TSessionType>> logger)
         : base(sessionManager, httpClientFactory, options, logger)
    {
    }

    protected override string PolicyHandlerName => nameof(EprNonRegulatorRolesPolicyHandler<TSessionType>);

    protected override string PolicyDescription =>
        "supports approved user, delegated user and basic user for admin and employee accounts";
    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
        ClaimsPrincipleHelper.IsApprovedDelegatedOrBasicPerson;
}