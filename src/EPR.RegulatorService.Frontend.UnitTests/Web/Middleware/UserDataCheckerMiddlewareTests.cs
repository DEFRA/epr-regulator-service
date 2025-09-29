using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;

using System.Security.Claims;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Middleware
{
    using Organisation = Common.Authorization.Models.Organisation;

    [TestClass]
    public class UserDataCheckerMiddlewareTests
    {
        private Mock<RequestDelegate> _requestDelegateMock = null!;
        private Mock<HttpRequest> _httpRequestMock = null!;
        private Mock<IRequestCookieCollection> _requestCookiesMock = null!;
        private Mock<HttpContext> _httpContextMock = null!;
        private Mock<IFacadeService> _facadeServiceMock = null!;
        private Mock<IConfiguration> _configurationMock = null!;
        private Mock<ISessionManager<JourneySession>> _sessionManagerMock = null!;
        private UserDataCheckerMiddleware _systemUnderTest;

        [TestInitialize]
        public void Setup()
        {
            var userData = new UserData
            {
                Id = Guid.NewGuid(),
                Organisations = new List<Organisation>
                {
                    new Organisation()
                }
            };

            string userDataString = JsonSerializer.Serialize(userData);
            var identity = new ClaimsIdentity();
            identity.AddClaims(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.UserData, userDataString)
            });
            var user = new ClaimsPrincipal(identity);

            _requestCookiesMock = new Mock<IRequestCookieCollection>();
            _httpRequestMock = new Mock<HttpRequest>();
            _httpRequestMock.Setup(x => x.Cookies).Returns(_requestCookiesMock.Object);

            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.Setup(x => x.User).Returns(user);

            _requestDelegateMock = new Mock<RequestDelegate>();
            _facadeServiceMock = new Mock<IFacadeService>();

            _configurationMock = new Mock<IConfiguration>();
            _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();

            SetupControllerName("UserDataControllerName");

            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(s => s.Value).Returns("/health");

            _configurationMock
                .Setup(c => c.GetSection("HealthCheckPath"))
                .Returns(sectionMock.Object);

            _systemUnderTest = new UserDataCheckerMiddleware(
                _facadeServiceMock.Object,
                _sessionManagerMock.Object,
                _configurationMock.Object
            );
        }

        [TestMethod]
        public async Task GivenInvokeAsync_WhenHomePathAndAuthenticated_ThenNoError()
        {
            // Arrange            
            _httpRequestMock.Setup(x => x.Path).Returns("/" + PagePath.FullName);
            _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
            _httpContextMock.Setup(x => x.User!.Identity!.IsAuthenticated).Returns(true);

            // Act
            await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

            // Assert
            _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenInvokeAsync_WhenHomePathAndNotAuthenticated_ThenNoError()
        {
            // Arrange
            _httpRequestMock.Setup(x => x.Path).Returns("/" + PagePath.FullName);
            _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
            _httpContextMock.Setup(x => x.User!.Identity!.IsAuthenticated).Returns(false);

            // Act
            await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

            // Assert
            _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task GivenInvokeAsync_WhenInvalidHomePathAndAuthenticated_ThenNoError()
        {
            // Arrange            
            _httpRequestMock.Setup(x => x.Path).Returns("/home");

            var authenticationServiceMock = new Mock<IAuthenticationService>();
            authenticationServiceMock.Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(authenticationServiceMock.Object);

            _httpContextMock.SetupGet(x => x.RequestServices).Returns(serviceProviderMock.Object);
            _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
            _httpContextMock.Setup(x => x.User!.Identity!.IsAuthenticated).Returns(true);

            var responseList = new List<OrganisationResponse>
            {
                new OrganisationResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "org.Name",
                    OrganisationRole = "org.OrganisationRole",
                    OrganisationType = "org.OrganisationType",
                    NationId = 1
                }
            };

            var content = new UserDataResponse
            {
                UserDetails = new UserDetails
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "FirstName",
                    LastName = "LastName",
                    Email = "test@testing.com",
                    RoleInOrganisation = "RoleInOrganisation",
                    EnrolmentStatus = "EnrolmentStatus",
                    ServiceRole = "ServiceRole",
                    Service = "Service",
                    ServiceRoleId = 0,
                    Organisations = responseList
                }
            };

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(content))
            };

            _facadeServiceMock.Setup(x => x.GetUserAccountDetails()).ReturnsAsync(httpResponseMessage);

            // Act
            await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

            // Assert
            _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);

            httpResponseMessage.Dispose();
        }

        [DataTestMethod]
        [DataRow("/health", "/health/status")]   // configured path matches -> middleware should short-circuit to next
        [DataRow("", "/health/status")]          // empty configured path -> middleware proceeds, still calls next
        public async Task GivenInvokeAsync_HealthCheckVariants_ShouldCallNext(string configuredHealthPath, string requestPath)
        {
            SetHealthCheckPath(configuredHealthPath);
            SetRequestPath(requestPath);

            var next = CreateVerifiableNext();

            await _systemUnderTest.InvokeAsync(_httpContextMock.Object, next.Object);

            next.Verify(rd => rd(It.IsAny<HttpContext>()), Times.Once);
        }

        [TestMethod]
        public async Task GivenInvokeAsync_WhenGetUserAccountDetailsFails_ShouldRedirect()
        {
            // Arrange: ensure facade fails
            var failureResponse = SetupFacadeFail(System.Net.HttpStatusCode.InternalServerError);

            // IMPORTANT: override the user from Setup() so they're authenticated but WITHOUT UserData
            var identity = new ClaimsIdentity(authenticationType: "mock");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, "Test User"));
            var authedNoUserData = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(c => c.User).Returns(authedNoUserData);

            var httpCtx = EnsureWritableResponse();
            SetRequestPath("/home");
            SetPathBase("/home");

            // Act
            await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

            // Assert
            Assert.AreEqual(StatusCodes.Status302Found, httpCtx.Response.StatusCode);
            _requestDelegateMock.Verify(rd => rd(It.IsAny<HttpContext>()), Times.Never);

            failureResponse.Dispose();
        }

        private void SetupControllerName(string controllerName)
        {
            var controllerActionDescriptor = new ControllerActionDescriptor { ControllerName = controllerName };

            var metadata = new List<object> { controllerActionDescriptor };

            _httpContextMock.Setup(x => x.Features.Get<IEndpointFeature>()!.Endpoint).Returns(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "EndpointName"));
        }

        private void SetHealthCheckPath(string value)
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s.Value).Returns(value);
            _configurationMock.Setup(c => c.GetSection("HealthCheckPath")).Returns(section.Object);
        }

        private void SetPathBase(string value)
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s.Value).Returns(value);
            _configurationMock.Setup(c => c.GetSection("PATH_BASE")).Returns(section.Object);
        }

        private void SetRequestPath(string path)
        {
            _httpRequestMock.Setup(r => r.Path).Returns(new PathString(path));
            _httpContextMock.Setup(c => c.Request).Returns(_httpRequestMock.Object);
        }

        private Mock<RequestDelegate> CreateVerifiableNext()
        {
            var next = new Mock<RequestDelegate>();
            next.Setup(rd => rd(It.IsAny<HttpContext>())).Returns(Task.CompletedTask).Verifiable();
            return next;
        }

        private DefaultHttpContext EnsureWritableResponse()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream(); // needed so Redirect() can write
            _httpContextMock.Setup(c => c.Response).Returns(ctx.Response);
            return ctx;
        }

        private HttpResponseMessage SetupFacadeFail(System.Net.HttpStatusCode code)
        {
            var response = new HttpResponseMessage(code);
            _facadeServiceMock.Setup(s => s.GetUserAccountDetails()).ReturnsAsync(response);
            return response;
        }
    }
}