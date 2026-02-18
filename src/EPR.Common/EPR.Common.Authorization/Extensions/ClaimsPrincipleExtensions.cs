namespace EPR.Common.Authorization.Extensions;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.Identity.Web;
using Models;

public static class ClaimsPrincipleExtensions
{
    public static void AddOrUpdateUserData(this ClaimsPrincipal claimsPrincipal, UserData values)
    {
        var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
        var claim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        if (claim != null)
        {
            claimsIdentity?.RemoveClaim(claim);
        }

        claimsIdentity?.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(values)));
    }

    public static Guid UserId(this ClaimsPrincipal user)
    {
        var objectId = user.Claims.Single(claim => claim.Type == ClaimConstants.ObjectId);

        if (Guid.TryParse(objectId.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static UserData GetUserData(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal.GetData<UserData>(ClaimTypes.UserData);

    public static T GetData<T>(this ClaimsPrincipal claimsPrincipal, string name)
    {
        var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
        var claim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == name);

        return claim != null ? JsonSerializer.Deserialize<T>(claim.Value) : default;
    }
}