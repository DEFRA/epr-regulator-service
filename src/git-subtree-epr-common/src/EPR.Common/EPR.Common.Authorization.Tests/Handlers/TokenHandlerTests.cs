namespace EPR.Common.Authorization.Test.Handlers;

using System.Security.Claims;
using Authorization.Handlers;
using Config;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestClasses;

[TestClass]
public class TokenHandlerTests
{
    private readonly Mock<ITokenAcquisition>? tokenAcquisitionMock = new();
    private readonly Mock<IOptions<EprAuthorizationConfig>>? optionsMock = new();

    [TestInitialize]
    public void Setup()
    {
        optionsMock.Setup(x => x.Value).Returns(new EprAuthorizationConfig
        {
            FacadeDownStreamScope = "dummy-scope",
        });
    }

    [TestMethod]
    public async Task SendAsync_AddsAuthorizationHeader()
    {
        string token = "dummy-token";
        tokenAcquisitionMock?.Setup(x => x.GetAccessTokenForUserAsync(
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()))
            .ReturnsAsync(token);

        var innerHandler = new FakeHttpMessageHandler(new HttpResponseMessage());
        var tokenHandler = new TokenHandler(tokenAcquisitionMock?.Object, optionsMock?.Object);
        tokenHandler.InnerHandler = innerHandler;
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");
        var invoker = new HttpMessageInvoker(tokenHandler);

        var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.IsNotNull(request.Headers.Authorization);
        Assert.AreEqual(request?.Headers?.Authorization?.Scheme, "Bearer");
        Assert.AreEqual(request?.Headers?.Authorization?.Parameter, token);
    }
}