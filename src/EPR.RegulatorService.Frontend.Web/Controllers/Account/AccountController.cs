using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.RegulatorService.Frontend.Web.Controllers.Account;

/// <summary>
/// Controller used in web apps to manage accounts.
/// </summary>
[AllowAnonymous]
[Route("[controller]/[action]")]
public class AccountController : Controller
{
    private readonly IOptionsMonitor<MicrosoftIdentityOptions> _optionsMonitor;

    /// <summary>
    /// Constructor of <see cref="AccountController"/> from <see cref="MicrosoftIdentityOptions"/>
    /// This constructor is used by dependency injection.
    /// </summary>
    /// <param name="microsoftIdentityOptionsMonitor">Configuration options.</param>
    public AccountController(IOptionsMonitor<MicrosoftIdentityOptions> microsoftIdentityOptionsMonitor)
    {
        _optionsMonitor = microsoftIdentityOptionsMonitor;
    }

    /// <summary>
    /// Handles user sign in.
    /// </summary>
    /// <param name="scheme">Authentication scheme.</param>
    /// <param name="redirectUri">Redirect URI.</param>
    /// <returns>Challenge generating a redirect to Azure AD to sign in the user.</returns>
    [HttpGet("{scheme?}")]
    public IActionResult SignIn(
        [FromRoute] string? scheme,
        [FromQuery] string redirectUri)
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
        string redirect;
        if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
        {
            redirect = Url.Content(redirectUri);
        }
        else
        {
            redirect = Url.Content("~/");
        }

        return Challenge(
            new AuthenticationProperties { RedirectUri = redirect },
            scheme);
    }

    /// <summary>
    /// Handles the user sign-out.
    /// </summary>
    /// <param name="scheme">Authentication scheme.</param>
    /// <returns>Sign out result.</returns>
    [HttpGet("{scheme?}")]
    public IActionResult SignOut(
        [FromRoute] string? scheme)
    {
        if (AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
        {
            if (AppServicesAuthenticationInformation.LogoutUrl != null)
            {
                return LocalRedirect(AppServicesAuthenticationInformation.LogoutUrl);
            }
            return Ok();
        }

        var selectedCulture = HttpContext.Session.GetString(Language.SessionLanguageKey);
        var queryParams = !string.IsNullOrEmpty(selectedCulture) ? new { culture = selectedCulture } : null;

        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
        var callbackUrl = Url.Action(action: "SignedOut", controller: "Home", values: queryParams, protocol: Request.Scheme);
        return SignOut(
             new AuthenticationProperties
             {
                 RedirectUri = callbackUrl,
             },
             CookieAuthenticationDefaults.AuthenticationScheme,
             scheme);

    }

    /// <summary>
    /// In B2C applications handles the Reset password policy.
    /// </summary>
    /// <param name="scheme">Authentication scheme.</param>
    /// <returns>Challenge generating a redirect to Azure AD B2C.</returns>
    [HttpGet("{scheme?}")]
    public IActionResult ResetPassword([FromRoute] string? scheme)
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;

        var redirectUrl = Url.Content("~/");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            Items =
            {
                [Microsoft.Identity.Web.Constants.Policy] = _optionsMonitor.Get(scheme).ResetPasswordPolicyId
            }
        };
        return Challenge(properties, scheme);
    }

    public IActionResult AccessDenied() => View();
}