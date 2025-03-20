using System.Net;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Errors;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations
{
    [TestClass]
    public class RegistrationsControllerTests
    {
        private const string BackLinkViewDataKey = "BackLinkToDisplay";
        private RegistrationsController _controller;
        private Mock<ISessionManager<JourneySession>> _mockSessionManager;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpContext> _httpContextMock = null!;


        [TestInitialize]
        public void TestInitialize()
        {
            _httpContextMock = new Mock<HttpContext>();
            _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
            _mockConfiguration = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var mockRequest = new Mock<HttpRequest>();
            var mockHeaders = new Mock<IHeaderDictionary>();

            // Set up the Referer header to return a sample URL (or null for different tests)
            mockHeaders.Setup(h => h["Referer"]).Returns("http://previous-page.com");

            // Set the mock Request to the HttpContext
            mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);
            _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

            configurationSectionMock
                .Setup(section => section.Value)
                .Returns("/regulators");

            _mockConfiguration
                .Setup(config => config.GetSection(ConfigKeys.PathBase))
                .Returns(configurationSectionMock.Object);

            _mockSessionManager
                .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession());

            _controller = new RegistrationsController(_mockSessionManager.Object, _mockConfiguration.Object);

            _controller.ControllerContext.HttpContext = _httpContextMock.Object;
        }

        [TestMethod]
        public async Task UkSiteDetails_WithCorrectModel_ShouldReturnView()
        {
            // Act
            var result = await _controller.UkSiteDetails();

            // Assert

            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();

                var viewResult = result as ViewResult;
                viewResult.Should().NotBeNull();
                viewResult!.Model.Should().BeOfType<ManageRegistrationsViewModel>();

                var model = viewResult.Model as ManageRegistrationsViewModel;
                model.Should().NotBeNull();
                model!.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
            }

        }

        [TestMethod]
        public async Task BusinessAddress_WithCorrectModel_ShouldReturnView()
        {
            // Act
            var result = await _controller.BusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();

                var viewResult = result as ViewResult;
                viewResult.Should().NotBeNull();
                viewResult!.Model.Should().BeOfType<ManageRegistrationsViewModel>();

                var model = viewResult.Model as ManageRegistrationsViewModel;
                model.Should().NotBeNull();
                model!.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Exporter);

                AssertBackLink(viewResult, PagePath.ManageRegistrations);
            }
        }

        [TestMethod]
        public async Task BusinessAddress_WithNullSession_ShouldRedirectToError()
        {
            // Arrange
            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync((JourneySession)null);

            // Act
            var result = await _controller.BusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task BusinessAddress_WithNullReprocessorExporterSession_ShouldRedirectToError()
        {
            // Arrange
            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(new JourneySession { ReprocessorExporterSession = null });

            // Act
            var result = await _controller.BusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task BusinessAddress_WithNullJourney_ShouldRedirectToError()
        {
            // Arrange
            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(new JourneySession { ReprocessorExporterSession = new ReprocessorExporterSession { Journey = null } });

            // Act
            var result = await _controller.BusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task AuthorisedMaterials_WithCorrectModel_ShouldReturnView()
        {
            // Act
            var result = await _controller.AuthorisedMaterials();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();

                var viewResult = result as ViewResult;
                viewResult.Should().NotBeNull();
                viewResult!.Model.Should().BeOfType<ManageRegistrationsViewModel>();

                var model = viewResult.Model as ManageRegistrationsViewModel;
                model.Should().NotBeNull();
                model!.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
            }

        }

        protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
        {
            var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
            hasBackLinkKey.Should().BeTrue();
            (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
        }
    }
}