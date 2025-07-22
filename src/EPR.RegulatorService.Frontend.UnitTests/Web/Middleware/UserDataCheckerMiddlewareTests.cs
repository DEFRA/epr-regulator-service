using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Security.Claims;
using System.Text.Json;
using System.Net;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Middleware
{

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
        protected Mock<ILogger<UserDataCheckerMiddleware>> _loggerMock = null!;

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
            _loggerMock = new Mock<ILogger<UserDataCheckerMiddleware>>();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(section => section.Value).Returns("/admin/health");
            _configurationMock.Setup(config => config.GetSection(ConfigKeys.HealthCheckPath))
                .Returns(configurationSectionMock.Object);

            _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();

            SetupControllerName("UserDataControllerName");

            _systemUnderTest = new UserDataCheckerMiddleware(
                _facadeServiceMock.Object,
                _sessionManagerMock.Object,
                _configurationMock.Object,
                _loggerMock.Object
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

        private void SetupControllerName(string controllerName)
        {
            var controllerActionDescriptor = new ControllerActionDescriptor { ControllerName = controllerName };

            var metadata = new List<object> { controllerActionDescriptor };

            _httpContextMock.Setup(x => x.Features.Get<IEndpointFeature>()!.Endpoint).Returns(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "EndpointName"));
        }
    }
}