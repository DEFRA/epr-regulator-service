using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

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
        mockHeaders.Setup(h => h.Referer).Returns("http://previous-page.com");
        
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
        }
    }

    [TestMethod]
    public async Task BusinessAddress_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.BusinessAddress();

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;
            
            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task BusinessAddress_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.BusinessAddress();
        });
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

    [TestMethod]
    public async Task UkSiteDetails_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.UkSiteDetails();
        });
    }

    [TestMethod]
    public async Task AuthorisedMaterials_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.AuthorisedMaterials();
        });
    }

    [TestMethod]
    public async Task AuthorisedMaterials_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null); // Simulating missing Referer header

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.AuthorisedMaterials();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task InputsAndOutputs_WithCorrectModel_ShouldReturnView()
    {
        // Act
        var result = await _controller.InputsAndOutputs();

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
    public async Task InputsAndOutputs_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange: Mock _sessionManager to return null
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.InputsAndOutputs();
        });
    }

    [TestMethod]
    public async Task SamplingInspection_WithCorrectModel_ShouldReturnView()
    {
        // Act
        var result = await _controller.SamplingInspection();

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
    public async Task SamplingInspection_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.SamplingInspection();

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.SamplingInspection();
        });
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.SamplingInspection();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task SamplingInspection_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.SamplingInspection();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task SamplingInspection_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.SamplingInspection();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WithCorrectModel_ShouldReturnView()
    {
        // Act
        var result = await _controller.OverseasReprocessorInterim();

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
        }
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.OverseasReprocessorInterim();

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.OverseasReprocessorInterim();
        });
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.OverseasReprocessorInterim();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.OverseasReprocessorInterim();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.OverseasReprocessorInterim();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task WasteLicences_WithCorrectModel_ShouldReturnView()
    {
        // Act
        var result = await _controller.WasteLicences();

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
        }
    }

    [TestMethod]
    public async Task WasteLicences_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.WasteLicences();

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task WasteLicences_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.WasteLicences();
        });
    }

    [TestMethod]
    public async Task WasteLicences_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.WasteLicences();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task WasteLicences_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.WasteLicences();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task WasteLicences_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.WasteLicences();

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }
    
    [TestMethod]
    public async Task MaterialWasteLicences_WithCorrectModel_ShouldReturnView()
    {
        // Act
        var result = await _controller.MaterialWasteLicences();

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
    public async Task MaterialWasteLicences_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.MaterialWasteLicences();
        });
    }

    [TestMethod]
    public async Task MaterialWasteLicences_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        const string expectedPreviousPage = $"{PagePath.ManageRegistrations}?id=1345";

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(expectedPreviousPage);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.MaterialWasteLicences();

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, expectedPreviousPage);
        }
    }

    private static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}
