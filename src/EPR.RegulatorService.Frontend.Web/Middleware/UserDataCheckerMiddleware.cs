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

    using Sessions;

    public class UserDataCheckerMiddleware : IMiddleware
    {
        private readonly IFacadeService _facadeService;
        private readonly IConfiguration _configuration;
        private readonly ISessionManager<JourneySession> _sessionManager;

        public UserDataCheckerMiddleware(IFacadeService facadeService,
            ISessionManager<JourneySession> sessionManager,
            IConfiguration configuration)
        {
            _facadeService = facadeService;
            _configuration = configuration;
            _sessionManager = sessionManager;
        }

        private static bool IsRegistrationRequest(HttpRequest request) =>
            request.Path.Value?.TrimStart('/') == PagePath.FullName;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path != _configuration.GetValue<string>(ConfigKeys.HealthCheckPath) && context.User.Identity is { IsAuthenticated: true } && context.User.TryGetUserData() is null)
            {
                if (IsRegistrationRequest(context.Request))
                {
                    await next(context);
                    return;
                }

                var result = await _facadeService.GetUserAccountDetails();
                var journeySession = new JourneySession();

                if (!result.IsSuccessStatusCode)
                {
                    context.Response.Redirect(_configuration.GetValue<string>("PATH_BASE"));
                    return;
                }

                var response = await result.Content.ReadFromJsonAsync<UserDataResponse>();

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

                journeySession.UserData = userData;

                await _sessionManager.SaveSessionAsync(context.Session, journeySession);

                await ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync(context, userData);
            }

            await next(context);
        }
    }
}
