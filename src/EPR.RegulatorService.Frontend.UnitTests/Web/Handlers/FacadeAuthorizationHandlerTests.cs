using EPR.RegulatorService.Frontend.Web.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using EPR.RegulatorService.Frontend.Core.Configs;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Handlers;

[TestClass]
public class FacadeAuthorizationHandlerTests
{
    [TestMethod]
    public async Task SendAsync_ShouldSetAuthorizationHeader_WhenCalled()
    {
        // Arrange
        const string expectedToken = "ExpectedToken";
        const string expectedDownstreamScope = "ExpectedDownstreamScope";

        var mockTokenAcquisition = new Mock<ITokenAcquisition>();
        mockTokenAcquisition
            .Setup(x => x.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions?>()))
            .ReturnsAsync(expectedToken);

        var mockFacadeConfig = new Mock<IOptions<FacadeApiConfig>>();
        mockFacadeConfig
            .Setup(m => m.Value)
            .Returns(new FacadeApiConfig { DownstreamScope = expectedDownstreamScope });

        var handler = new TestableFacadeAuthorizationHandler(mockTokenAcquisition.Object, mockFacadeConfig.Object)
        {
            InnerHandler = new TestHandler()
        };

        var httpRequestMessage = new HttpRequestMessage();

        // Act
        await handler.PublicSendAsync(httpRequestMessage, CancellationToken.None);

        // Assert
        httpRequestMessage.Headers.Authorization.Should().NotBeNull();
        httpRequestMessage.Headers.Authorization?.Scheme.Should().Be("Bearer");
        httpRequestMessage.Headers.Authorization?.Parameter.Should().Be(expectedToken);

        mockTokenAcquisition.Verify(x => x.GetAccessTokenForUserAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            It.IsAny<TokenAcquisitionOptions?>()), Times.Once);

        handler.Dispose();
        httpRequestMessage.Dispose();
    }

    private class TestableFacadeAuthorizationHandler : FacadeAuthorizationHandler
    {
        public TestableFacadeAuthorizationHandler(ITokenAcquisition tokenAcquisition, IOptions<FacadeApiConfig> options) : base(tokenAcquisition, options)
        {
        }

        public async Task<HttpResponseMessage> PublicSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await SendAsync(request, cancellationToken);
        }
    }

    private class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}