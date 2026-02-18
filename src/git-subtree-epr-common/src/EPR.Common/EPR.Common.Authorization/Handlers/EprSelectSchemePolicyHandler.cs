namespace EPR.Common.Authorization.Handlers;

using System.Security.Claims;
using Config;
using Helpers;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Requirements;
using Sessions;

public sealed class EprSelectSchemePolicyHandler<TSessionType>
    : PolicyHandlerBase<EprSelectSchemePolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    public EprSelectSchemePolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<EprSelectSchemePolicyHandler<TSessionType>> logger)
        : base(sessionManager, httpClientFactory, options, logger)
    {
    }

    protected override string PolicyHandlerName => nameof(EprSelectSchemePolicyHandler<TSessionType>);
    protected override string PolicyDescription => "select compliance scheme";
    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
        ClaimsPrincipleHelper.IsApprovedOrDelegatedPerson;
}