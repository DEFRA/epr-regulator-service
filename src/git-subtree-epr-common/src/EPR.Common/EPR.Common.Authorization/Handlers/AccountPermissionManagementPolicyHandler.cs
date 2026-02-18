namespace EPR.Common.Authorization.Handlers;

using System.Security.Claims;
using Config;
using Helpers;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Requirements;
using Sessions;

public sealed class AccountPermissionManagementPolicyHandler<TSessionType>
    : PolicyHandlerBase<AccountPermissionManagementPolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    public AccountPermissionManagementPolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<AccountPermissionManagementPolicyHandler<TSessionType>> logger)
        : base(sessionManager, httpClientFactory, options, logger)
    {
    }

    protected override string PolicyHandlerName => nameof(AccountPermissionManagementPolicyHandler<TSessionType>);
    protected override string PolicyDescription => "manage permissions";
    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
        ClaimsPrincipleHelper.IsEnrolledAdmin;
}