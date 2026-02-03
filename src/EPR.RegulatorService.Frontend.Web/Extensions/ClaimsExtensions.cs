namespace EPR.RegulatorService.Frontend.Web.Extensions
{
    using System.Security.Claims;
    using System.Text.Json;

    using Common.Authorization.Extensions;
    using Common.Authorization.Models;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Identity.Web;

    public static class ClaimsExtensions
    {
        public static UserData GetUserData(this ClaimsPrincipal claimsPrincipal)
        {
            var userDataClaim = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.UserData);

            return JsonSerializer.Deserialize<UserData>(userDataClaim.Value);
        }

        public static UserData? TryGetUserData(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.GetUserData();
        }

        public static UserData GetInvitedUserData(this ClaimsPrincipal claimsPrincipal)
        {
            var idClaim = claimsPrincipal.Claims.Single(claim => claim.Type == ClaimConstants.ObjectId);
            var emailClaim = claimsPrincipal.Claims.Single(claim => claim.Type == ClaimTypes.Email);

            return new UserData
            {
                Id = Guid.Parse(idClaim.Value),
                Email = emailClaim.Value
            };
        }

        public static async Task UpdateUserDataClaimsAndSignInAsync(HttpContext httpContext, UserData userData)
        {
            var claimsIdentity = httpContext.User.Identity as ClaimsIdentity;
            var userDataClaims = claimsIdentity?.FindAll(ClaimTypes.UserData).ToList();
            userDataClaims?.ForEach(claim => claimsIdentity?.RemoveClaim(claim));

            var claims = new List<Claim> { new(ClaimTypes.UserData, JsonSerializer.Serialize(userData)) };
            claimsIdentity = new ClaimsIdentity(httpContext.User.Identity, claims);
            var principal = new ClaimsPrincipal(claimsIdentity);
            var properties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            httpContext.User.AddOrUpdateUserData(userData);
        }
    }
}
