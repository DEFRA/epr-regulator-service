namespace EPR.Common.Authorization.Handlers;

using System.Security.Claims;
using Config;
using Helpers;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Requirements;
using Sessions;

public sealed class EprFileUploadPolicyHandler<TSessionType>
    : PolicyHandlerBase<EprFileUploadPolicyRequirement, TSessionType>
    where TSessionType : class, IHasUserData, new()
{
    public EprFileUploadPolicyHandler(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClient,
        IOptions<EprAuthorizationConfig> options,
        ILogger<EprFileUploadPolicyHandler<TSessionType>> logger)
        : base(sessionManager, httpClient, options, logger)
    {
    }

    protected override string PolicyHandlerName => nameof(EprFileUploadPolicyHandler<TSessionType>);
    protected override string PolicyDescription => "upload file";
    protected override Func<ClaimsPrincipal, bool> IsUserAllowed =>
        ClaimsPrincipleHelper.CanUploadFilesForOrganisation;
}