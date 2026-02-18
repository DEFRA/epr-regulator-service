namespace EPR.Common.Functions.Test.AccessControl;

using FluentAssertions;
using Functions.AccessControl;
using Functions.CancellationTokens.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

[TestClass]
public class HttpAuthenticatorTests
{
    private HttpAuthenticator httpAuthenticator;
    private ILogger<HttpAuthenticator> logger;
    private ICancellationTokenAccessor cancellationTokenAccessor;

    [TestInitialize]
    public void Setup()
    {
        this.logger = Substitute.For<ILogger<HttpAuthenticator>>();
        this.cancellationTokenAccessor = Substitute.For<ICancellationTokenAccessor>();
        this.httpAuthenticator = new HttpAuthenticator(this.logger, this.cancellationTokenAccessor);
    }

    [TestMethod]
    public async Task AuthenticateAsync_ReturnsFalse_WhenBearerTokenIsNull()
    {
        // Arrange
        string bearerToken = null;

        // Act
        var result = await this.httpAuthenticator.AuthenticateAsync(bearerToken);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public async Task AuthenticateAsync_ReturnsTrue_AndExtractsClaims_WhenTokenValid()
    {
        // Arrange - create a minimal JWT with required claims
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}")).TrimEnd('=');
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{{\"uniqueReference\":\"{userId}\",\"email\":\"test@test.com\",\"customerOrganisationId\":\"{orgId}\",\"customerId\":\"{customerId}\"}}")).TrimEnd('=');
        var token = $"Bearer {header}.{payload}.";

        // Act
        var result = await this.httpAuthenticator.AuthenticateAsync(token);

        // Assert
        result.Should().BeTrue();
        this.httpAuthenticator.UserId.Should().Be(userId);
        this.httpAuthenticator.EmailAddress.Should().Be("test@test.com");
        this.httpAuthenticator.CustomerOrganisationId.Should().Be(orgId);
        this.httpAuthenticator.CustomerId.Should().Be(customerId);
    }

    [TestMethod]
    public void UserId_ThrowsNotSupportedException_WhenNotAuthenticated()
    {
        var act = () => this.httpAuthenticator.UserId;

        act.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void EmailAddress_ThrowsNotSupportedException_WhenNotAuthenticated()
    {
        var act = () => this.httpAuthenticator.EmailAddress;

        act.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void CustomerOrganisationId_ThrowsNotSupportedException_WhenNotAuthenticated()
    {
        var act = () => this.httpAuthenticator.CustomerOrganisationId;

        act.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void CustomerId_ThrowsNotSupportedException_WhenNotAuthenticated()
    {
        var act = () => this.httpAuthenticator.CustomerId;

        act.Should().Throw<NotSupportedException>();
    }
}