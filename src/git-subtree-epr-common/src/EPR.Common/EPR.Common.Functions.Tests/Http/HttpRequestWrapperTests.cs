namespace EPR.Common.Functions.Test.Http;

using System.Net;
using EPR.Common.Functions.AccessControl.Interfaces;
using EPR.Common.Functions.CancellationTokens.Interfaces;
using EPR.Common.Functions.Exceptions;
using EPR.Common.Functions.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

[TestClass]
public class HttpRequestWrapperTests
{
    private ILogger<HttpRequestWrapper<string>> logger;
    private IAuthenticator authenticator;
    private IHttpContextAccessor httpContextAccessor;
    private IUserContextProvider userContextProvider;
    private ICancellationTokenAccessor cancellationTokenAccessor;
    private HttpRequestWrapper<string> wrapper;
    private DefaultHttpContext httpContext;

    [TestInitialize]
    public void Setup()
    {
        this.logger = Substitute.For<ILogger<HttpRequestWrapper<string>>>();
        this.authenticator = Substitute.For<IAuthenticator>();
        this.httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        this.userContextProvider = Substitute.For<IUserContextProvider>();
        this.cancellationTokenAccessor = Substitute.For<ICancellationTokenAccessor>();

        this.httpContext = new DefaultHttpContext();
        this.httpContextAccessor.HttpContext.Returns(this.httpContext);

        this.wrapper = new HttpRequestWrapper<string>(
            this.logger,
            this.authenticator,
            this.httpContextAccessor,
            this.userContextProvider,
            this.cancellationTokenAccessor);
    }

    [TestMethod]
    public async Task Execute_ReturnsUnauthorized_WhenAuthenticationFails()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(false);

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => Task.FromResult<ActionResult<string>>(new OkObjectResult("ok")),
            CancellationToken.None);

        result.Result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result.Result).StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task Execute_ReturnsResult_WhenAuthenticationSucceeds()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => Task.FromResult<ActionResult<string>>(new OkObjectResult("success")),
            CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Execute_ReturnsNotFound_WhenEntityNotFoundExceptionThrown()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => throw new EntityNotFoundException("not found"),
            CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task Execute_ReturnsBadRequest_WhenBadRequestExceptionThrown()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => throw new BadRequestException("bad request"),
            CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [TestMethod]
    public async Task Execute_ReturnsEmpty_WhenOperationCancelled()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => throw new OperationCanceledException(cts.Token),
            cts.Token);

        result.Result.Should().BeOfType<EmptyResult>();
    }

    [TestMethod]
    public async Task Execute_Returns500_WhenUnhandledExceptionThrown()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        result.Result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result.Result).StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task Execute_Returns500_WhenAuthenticationThrows()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).ThrowsAsync(new Exception("auth failed"));

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => Task.FromResult<ActionResult<string>>(new OkObjectResult("ok")),
            CancellationToken.None);

        result.Result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result.Result).StatusCode.Should().Be(500);
    }

    [TestMethod]
    public async Task Execute_UsesExistingCorrelationHeader_WhenPresent()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());
        this.httpContext.Request.Headers["x-epr-request-id"] = "existing-id";

        var result = await this.wrapper.Execute<string>(
            new List<string>(),
            () => Task.FromResult<ActionResult<string>>(new OkObjectResult("ok")),
            CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [TestMethod]
    public async Task Execute_NonGeneric_ReturnsResult()
    {
        this.authenticator.AuthenticateAsync(Arg.Any<string>()).Returns(true);
        this.userContextProvider.UserId.Returns(Guid.NewGuid());

        var result = await this.wrapper.Execute(
            new List<string>(),
            () => Task.FromResult<ActionResult>(new OkResult()),
            CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }
}
