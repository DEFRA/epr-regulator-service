namespace EPR.Common.Authorization.Handlers;

using System.Net.Http.Headers;
using Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

public class TokenHandler : DelegatingHandler
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string[] _scopes;

    public TokenHandler(ITokenAcquisition tokenAcquisition, IOptions<EprAuthorizationConfig> options)
    {
        _tokenAcquisition = tokenAcquisition;
        _scopes = new[]
        {
            options.Value.FacadeDownStreamScope,
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}