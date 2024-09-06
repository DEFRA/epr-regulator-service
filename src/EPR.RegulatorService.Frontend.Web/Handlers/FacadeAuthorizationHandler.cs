using System.Net.Http.Headers;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.RegulatorService.Frontend.Web.Handlers;

using Core.Configs;

public class FacadeAuthorizationHandler : DelegatingHandler
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string[] _scopes;

    public FacadeAuthorizationHandler(ITokenAcquisition tokenAcquisition, IOptions<FacadeApiConfig> options)
    {
        _tokenAcquisition = tokenAcquisition;
        _scopes = new[]
        {
            options.Value.DownstreamScope,
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}