namespace EPR.Common.Authorization.Handlers;

using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Config;
using Constants;
using Extensions;
using Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Sessions;

public abstract class PolicyHandlerBase<TPolicyRequirement, TSessionType>
    : AuthorizationHandler<TPolicyRequirement>
    where TPolicyRequirement : IAuthorizationRequirement
    where TSessionType : class, IHasUserData, new()
{
    private readonly ISessionManager<TSessionType> _sessionManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EprAuthorizationConfig _config;
    private readonly ILogger<PolicyHandlerBase<TPolicyRequirement, TSessionType>> _logger;

    protected PolicyHandlerBase(
        ISessionManager<TSessionType> sessionManager,
        IHttpClientFactory httpClientFactory,
        IOptions<EprAuthorizationConfig> options,
        ILogger<PolicyHandlerBase<TPolicyRequirement, TSessionType>> logger)
    {
        _sessionManager = sessionManager;
        _httpClientFactory = httpClientFactory;
        _config = options.Value;
        _logger = logger;
    }

    protected abstract string PolicyHandlerName { get; }
    protected abstract string PolicyDescription { get; }
    protected abstract Func<ClaimsPrincipal, bool> IsUserAllowed { get; }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TPolicyRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User is unauthenticated");
            return;
        }

        // check context
        if (CheckContext(context, requirement))
            return;

        // check cache
        if (context.Resource is not HttpContext httpContext)
        {
            _logger.LogError("Error getting HttpContext in {PolicyHandler} for user {UserId}",
                PolicyHandlerName, context.User.UserId());
            return;
        }

        var cacheResult = await CheckCache(context, requirement, httpContext);
        if (cacheResult.InCache)
            return;

        // check db via facade
        if (await CheckDatabase(context, requirement, cacheResult.Session, httpContext))
            return;

        _logger.LogWarning("User {UserId} does not have permission to {PolicyDescription}",
            context.User.UserId(), PolicyDescription);
    }

    private async Task<bool> CheckDatabase(
        AuthorizationHandlerContext context,
        TPolicyRequirement requirement,
        TSessionType session,
        HttpContext httpContext)
    {
        using var httpClient = _httpClientFactory.CreateClient(FacadeConstants.FacadeAPIClient);
        var dbResponse = await httpClient.GetFromJsonAsync<UserOrganisations>(_config.FacadeUserAccountEndpoint);
        if (dbResponse == null)
            return false;

        context.User.AddOrUpdateUserData(dbResponse.User);
        session.UserData = dbResponse.User;

        await _sessionManager.SaveSessionAsync(httpContext.Session, session);
        await UpdateClaimsAndSignInAsync(httpContext, dbResponse.User);

        if (!IsUserAllowed(context.User))
            return false;

        context.Succeed(requirement);
        _logger.LogInformation("User {UserId} has permission to {PolicyDescription}",
            context.User.UserId(), PolicyDescription);
        return true;
    }

    private async Task<(bool InCache, TSessionType Session)> CheckCache(
        AuthorizationHandlerContext context,
        TPolicyRequirement requirement,
        HttpContext httpContext)
    {
        TSessionType consumerSession = await _sessionManager.GetSessionAsync(httpContext.Session) ?? new TSessionType();

        if (consumerSession is not { UserData: { } })
        {
            return (InCache: false, Session: consumerSession);
        }

        context.User.AddOrUpdateUserData(consumerSession.UserData);

        await UpdateClaimsAndSignInAsync(httpContext, consumerSession.UserData);

        if (IsUserAllowed(context.User))
        {
            context.Succeed(requirement);
            _logger.LogInformation("User {UserId} has permission to {PolicyDescription}",
                context.User.UserId(), PolicyDescription);
            return (true, consumerSession);
        }

        return (false, consumerSession);
    }

    private bool CheckContext(AuthorizationHandlerContext context, TPolicyRequirement requirement)
    {
        if (!IsUserAllowed(context.User))
        {
            return false;
        }

        context.Succeed(requirement);
        _logger.LogInformation("User {UserId} has permission to {PolicyDescription}",
            context.User.UserId(), PolicyDescription);
        return true;
    }

    private async Task UpdateClaimsAndSignInAsync(HttpContext httpContext, UserData userData)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.UserData, JsonSerializer.Serialize(userData)),
        };
        var claimsIdentity = new ClaimsIdentity(httpContext.User.Identity, claims);
        var principal = new ClaimsPrincipal(claimsIdentity);
        var properties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        if (!string.IsNullOrEmpty(_config.SignInRedirect))
        {
            httpContext.Response.Redirect(_config.SignInRedirect);
        }
    }
}