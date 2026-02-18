namespace EPR.Common.Functions.Test.Controllers;

using EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;
using EPR.Common.Functions.AccessControl.MockAuthentication.Models;
using EPR.Common.Functions.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

[TestClass]
public class AuthControllerTests
{
    private IJwtAuthenticationManager jwtAuthenticationManager;
    private IJwtTokenRefresher tokenRefresher;
    private IConfiguration configuration;
    private AuthController controller;

    [TestInitialize]
    public void Setup()
    {
        this.jwtAuthenticationManager = Substitute.For<IJwtAuthenticationManager>();
        this.tokenRefresher = Substitute.For<IJwtTokenRefresher>();
        this.configuration = Substitute.For<IConfiguration>();
        this.controller = new AuthController(this.jwtAuthenticationManager, this.tokenRefresher, this.configuration);
    }

    [TestMethod]
    public void Authenticate_ReturnsOk_WhenCredentialsValid()
    {
        var credentials = new UserCredentials { Username = "user", Password = "pass" };
        var response = new AuthenticationResponse { JwtToken = "token", RefreshToken = "refresh" };
        this.jwtAuthenticationManager.Authenticate("user", "pass").Returns(response);

        var result = this.controller.Authenticate(credentials);

        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(response);
    }

    [TestMethod]
    public void Authenticate_ReturnsUnauthorized_WhenCredentialsInvalid()
    {
        var credentials = new UserCredentials { Username = "user", Password = "wrong" };
        this.jwtAuthenticationManager.Authenticate("user", "wrong").Returns((AuthenticationResponse)null);

        var result = this.controller.Authenticate(credentials);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [TestMethod]
    public void Refresh_ReturnsOk_WhenTokenValid()
    {
        var credentials = new RefreshCredential { JwtToken = "jwt", RefreshToken = "refresh" };
        var response = new AuthenticationResponse { JwtToken = "newToken", RefreshToken = "newRefresh" };
        this.tokenRefresher.Refresh(credentials).Returns(response);

        var result = this.controller.Refresh(credentials);

        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(response);
    }

    [TestMethod]
    public void Refresh_ReturnsUnauthorized_WhenTokenInvalid()
    {
        var credentials = new RefreshCredential { JwtToken = "jwt", RefreshToken = "bad" };
        this.tokenRefresher.Refresh(credentials).Returns((AuthenticationResponse)null);

        var result = this.controller.Refresh(credentials);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
