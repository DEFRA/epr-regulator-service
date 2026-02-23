namespace IntegrationTests.Infrastructure;

using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

public class MockTokenAcquisition : ITokenAcquisition
{
    public Task<string> GetAccessTokenForAppAsync(string scope, string? authenticationScheme, string? tenant = null, TokenAcquisitionOptions? tokenAcquisitionOptions = null) => Task.FromResult("mock-app-token");

    public Task<string> GetAccessTokenForUserAsync(IEnumerable<string> scopes, string? authenticationScheme, string? tenantId = null, string? userFlow = null, ClaimsPrincipal? user = null, TokenAcquisitionOptions? tokenAcquisitionOptions = null) => Task.FromResult("mock-user-token");

    public Task<AuthenticationResult> GetAuthenticationResultForAppAsync(string scope, string? authenticationScheme, string? tenant = null, TokenAcquisitionOptions? tokenAcquisitionOptions = null) => Task.FromResult<AuthenticationResult>(null!);

    public Task<AuthenticationResult> GetAuthenticationResultForUserAsync(IEnumerable<string> scopes, string? authenticationScheme, string? tenantId = null, string? userFlow = null, ClaimsPrincipal? user = null, TokenAcquisitionOptions? tokenAcquisitionOptions = null) => Task.FromResult<AuthenticationResult>(null!);

    public void ReplyForbiddenWithWwwAuthenticateHeader(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException, string? authenticationScheme, HttpResponse? httpResponse = null)
    {
        // No-op
    }

    public Task ReplyForbiddenWithWwwAuthenticateHeaderAsync(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException, HttpResponse? httpResponse = null) => Task.CompletedTask;

    public string GetEffectiveAuthenticationScheme(string? authenticationScheme) => authenticationScheme ?? "TestScheme";
}
