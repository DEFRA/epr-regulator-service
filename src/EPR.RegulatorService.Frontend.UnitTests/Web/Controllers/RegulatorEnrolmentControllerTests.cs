namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using System.Security.Claims;
    using System.Text.Json;

    using Frontend.Core.Models;
    using Frontend.Core.Services;
    using Frontend.Core.Sessions;
    using Frontend.Web.Constants;
    using Frontend.Web.Controllers.RegulatorEnrolment;
    using Frontend.Web.ViewModels.RegulatorEnrolment;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Web;

    using Moq;

    [TestClass]
    public class RegulatorEnrolmentControllerTests
    {
        private Mock<HttpContext> _httpContextMock;
        private Mock<ILogger<RegulatorEnrolmentController>> _loggerMock;
        private RegulatorEnrolmentController _systemUnderTest;
        private JourneySession _journeySession;
        private Mock<IFacadeService> _facadeServiceMock;
        private const string OrganisationName = "ACME";

        [TestInitialize]
        public void Setup()
        {
            _httpContextMock = new Mock<HttpContext>();
            _facadeServiceMock = new Mock<IFacadeService>();
            _loggerMock = new Mock<ILogger<RegulatorEnrolmentController>>();

            _journeySession = new JourneySession
            {
                UserData =
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Organisations = new List<Common.Authorization.Models.Organisation> { new() { Name = OrganisationName } },
                    ServiceRoleId = (int)Frontend.Core.Enums.ServiceRole.RegulatorAdmin
                }
            };

            var httpContext = new DefaultHttpContext();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimConstants.ObjectId, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(_journeySession.UserData)),
            }, "TestAuthentication"));

            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["SessionVariable"] = "Token"
            };

            _systemUnderTest = new RegulatorEnrolmentController(
                _loggerMock.Object,
                _facadeServiceMock.Object)
            {
                TempData = tempData
            };

            var authenticationServiceMock = new Mock<IAuthenticationService>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationServiceMock.Object);

            serviceProviderMock.Setup(_ => _.GetService(typeof(IUrlHelperFactory)))
                .Returns(Mock.Of<IUrlHelperFactory>());

            authenticationServiceMock
                .Setup(x => x.SignInAsync(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            _httpContextMock
                .Setup(x => x.User)
                .Returns(user);

            _httpContextMock
                .SetupGet(x => x.Features)
                .Returns(Mock.Of<IFeatureCollection>());

            _httpContextMock
                .SetupGet(x => x.RequestServices)
                .Returns(serviceProviderMock.Object);

            var httpRequestMock = new Mock<HttpRequest>();

            httpRequestMock.Setup(request => request.Path).Returns(new PathString("/full-name"));

            httpRequestMock.Setup(request => request.PathBase).Returns(new PathString("/regulators"));

            _httpContextMock.Setup(context => context.Request).Returns(httpRequestMock.Object);

            _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        }

        [TestMethod]
        public async Task FullName_WhenTokenIsNull_ReturnRedirectHome()
        {
            // Act
            var result = await _systemUnderTest.FullName(string.Empty);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = result as RedirectToActionResult;
            redirectResult!.ActionName.Should().Be(PagePath.Error);
        }

        
        [TestMethod]
        public async Task FullName_WhenTokenIsProvided_ReturnView()
        {
            // Act
            var result = await _systemUnderTest.FullName("Token");

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<FullNameViewModel>();
        }

        [TestMethod]
        public async Task FullName_WhenValidModelIsProvided_ReturnSuccessfulRedirectResult()
        {
            // Arrange
            var model = new FullNameViewModel
            {
                FirstName = "Test",
                LastName = "User",
            };

            _facadeServiceMock.Setup(sm => sm.EnrolInvitedUser(It.IsAny<EnrolInvitedUserRequest>()))
               .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _systemUnderTest.FullName(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = result as RedirectToActionResult;
            redirectResult!.ActionName.Should().Be(PagePath.LandingPage);

            _systemUnderTest.ModelState.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public async Task FullName_WhenAnInValidModelIsProvided_ReturnFailWithCustomError()
        {
            // Arrange
            var model = new FullNameViewModel
            {
                FirstName = "Test",
                LastName = "User",
            };

            _facadeServiceMock.Setup(sm => sm.EnrolInvitedUser(It.IsAny<EnrolInvitedUserRequest>()))
               .ReturnsAsync(EndpointResponseStatus.Fail);

            // Act
            var result = await _systemUnderTest.FullName(model);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<FullNameViewModel>();

            _systemUnderTest.ModelState.IsValid.Should().BeFalse();

            bool any = _systemUnderTest.ModelState["Error"]!.Errors
                .Any(modelError => modelError.ErrorMessage == "Unable to enrol invited user");

            any.Should().BeTrue();
        }
    }
}