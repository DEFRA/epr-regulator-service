namespace EPR.RegulatorService.Frontend.Web.Middleware
{
    using System.Threading.Tasks;

    using Common.Authorization.Models;

    using Constants;

    using Core.Models;
    using Core.Services;
    using Core.Sessions;

    using Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Sessions;

    public class UserDataCheckerMiddleware : IMiddleware
    {
        private readonly IFacadeService _facadeService;
        private readonly IConfiguration _configuration;
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly ILogger<UserDataCheckerMiddleware> _logger;

        public UserDataCheckerMiddleware(IFacadeService facadeService,
            ISessionManager<JourneySession> sessionManager,
            IConfiguration configuration,
            ILogger<UserDataCheckerMiddleware> logger)
        {
            _facadeService = facadeService;
            _configuration = configuration;
            _sessionManager = sessionManager;
            _logger = logger;
        }

        private static bool IsRegistrationRequest(HttpRequest request) =>
            request.Path.Value?.TrimStart('/') == PagePath.FullName;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogWarning("UserDataCheckerMiddleware: START - Path={Path}, IsAuthenticated={IsAuthenticated}",
                context.Request.Path, context.User.Identity?.IsAuthenticated);

            string healthCheckPath = _configuration.GetValue<string>("HealthCheckPath") ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(healthCheckPath) &&
                context.Request.Path.StartsWithSegments(healthCheckPath, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            var existingData = context.User.TryGetUserData();
            _logger.LogWarning("UserDataCheckerMiddleware: ExistingUserData={HasData}", existingData != null);

            if (context.User.Identity is { IsAuthenticated: true } && context.User.TryGetUserData() is null)
            {
                _logger.LogInformation("UserDataCheckerMiddleware: User is authenticated but has no UserData claim. Fetching from facade API. Path: {Path}", context.Request.Path);

                if (IsRegistrationRequest(context.Request))
                {
                    _logger.LogInformation("UserDataCheckerMiddleware: Registration request - skipping user data fetch");
                    await next(context);
                    return;
                }

                var result = await _facadeService.GetUserAccountDetails();
                var journeySession = new JourneySession();

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("UserDataCheckerMiddleware: Failed to fetch user account details from facade API. Status: {StatusCode}", result.StatusCode);
                    throw new Exception("facade auth failure");
                    context.Response.Redirect(_configuration.GetValue<string>("PATH_BASE"));
                    return;
                }

                var response = await result.Content.ReadFromJsonAsync<UserDataResponse>();

                _logger.LogInformation("UserDataCheckerMiddleware: Received user account details. UserId: {UserId}, ServiceRole: {ServiceRole}, ServiceRoleId: {ServiceRoleId}, OrganisationCount: {OrgCount}",
                    response!.UserDetails.Id,
                    response.UserDetails.ServiceRole,
                    response.UserDetails.ServiceRoleId,
                    response.UserDetails.Organisations?.Count ?? 0);

                var userData = new UserData
                {
                    Id = Guid.Parse(response!.UserDetails.Id),
                    FirstName = response.UserDetails.FirstName,
                    LastName = response.UserDetails.LastName,
                    Email = response.UserDetails.Email,
                    RoleInOrganisation = response.UserDetails.RoleInOrganisation,
                    EnrolmentStatus = response.UserDetails.EnrolmentStatus,
                    ServiceRole = response.UserDetails.ServiceRole,
                    Service = response.UserDetails.Service,
                    ServiceRoleId = response.UserDetails.ServiceRoleId,
                    Organisations = response.UserDetails.Organisations.Select(org => new Common.Authorization.Models.Organisation
                    {
                        Id = Guid.Parse(org.Id),
                        Name = org.Name,
                        OrganisationRole = org.OrganisationRole,
                        OrganisationType = org.OrganisationType,
                        NationId = org.NationId
                    }).ToList(),
                };

                if (userData.Organisations.Any())
                {
                    _logger.LogInformation("UserDataCheckerMiddleware: User organisations: {Organisations}",
                        string.Join(", ", userData.Organisations.Select(o => $"{o.Name} ({o.OrganisationRole})")));
                }

                journeySession.UserData = userData;

                await _sessionManager.SaveSessionAsync(context.Session, journeySession);

                await ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync(context, userData);

                _logger.LogWarning("UserDataCheckerMiddleware: Successfully updated user data claims. ServiceRole={ServiceRole}, ServiceRoleId={ServiceRoleId}",
                    userData.ServiceRole, userData.ServiceRoleId);
            }
            else if (context.User.Identity is { IsAuthenticated: true })
            {
                var existingUserData = context.User.TryGetUserData();
                if (existingUserData != null)
                {
                    _logger.LogDebug("UserDataCheckerMiddleware: User already has UserData claim. ServiceRole: {ServiceRole}, ServiceRoleId: {ServiceRoleId}",
                        existingUserData.ServiceRole,
                        existingUserData.ServiceRoleId);
                }
            }
            else if (context.User.Identity is { IsAuthenticated: false })
            {
                _logger.LogWarning("UserDataCheckerMiddleware: User is not authenticated for path: {Path}", context.Request.Path);
            }

            await next(context);
        }
    }
}
