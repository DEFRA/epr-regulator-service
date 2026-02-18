using System.Security.Claims;
using EPR.Common.Authorization.Config;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Helpers;
using EPR.Common.Authorization.Interfaces;
using EPR.Common.Authorization.Requirements;
using EPR.Common.Authorization.Sessions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Common.Authorization.Handlers;

public sealed class RegulatorAdminPolicyHandler<TSessionType>
    : PolicyHandlerBase<RegulatorAdminPolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    public RegulatorAdminPolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<RegulatorAdminPolicyHandler<TSessionType>> logger)
        : base(sessionManager, httpClientFactory, options, logger)
    {
    }

    protected override string PolicyHandlerName => nameof(RegulatorAdminPolicyHandler<TSessionType>);
    protected override string PolicyDescription => ServiceRoles.RegulatorAdmin;
    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
        ClaimsPrincipleHelper.IsRegulatorAdmin;
}