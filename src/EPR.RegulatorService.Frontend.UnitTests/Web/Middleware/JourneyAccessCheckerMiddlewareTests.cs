using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Attributes;
using EPR.RegulatorService.Frontend.Web.Middleware;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Middleware;

[TestClass]
public class JourneyAccessCheckerMiddlewareTests
{
    private Mock<RequestDelegate> _requestDelegateMock = null!;
    private IConfiguration _configuration = null!;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock = null!;
    private Mock<ISession> _sessionMock = null!;
    private Mock<IEndpointFeature> _endpointFeatureMock = null!;
    private JourneyAccessCheckerMiddleware _systemUnderTest = null!;
    private DefaultHttpContext _httpContext = null!;

    [TestInitialize]
    public void Setup()
    {
        _requestDelegateMock = new Mock<RequestDelegate>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _sessionMock = new Mock<ISession>();
        _endpointFeatureMock = new Mock<IEndpointFeature>();

        // Create a real configuration with empty HealthCheckPath by default
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "HealthCheckPath", string.Empty }
        });
        _configuration = configurationBuilder.Build();

        _httpContext = new DefaultHttpContext();
        _httpContext.Session = _sessionMock.Object;
        _httpContext.Features.Set<IEndpointFeature>(_endpointFeatureMock.Object);
        _httpContext.Request.PathBase = PathString.Empty;
        _httpContext.Response.Body = new MemoryStream();

        // Set up routing feature with empty route data
        var routeData = new RouteData();
        var routingFeature = new TestRoutingFeature { RouteData = routeData };
        _httpContext.Features.Set<IRoutingFeature>(routingFeature);

        _systemUnderTest = new JourneyAccessCheckerMiddleware(
            _requestDelegateMock.Object,
            _configuration);
    }

    private IConfiguration CreateConfiguration(string healthCheckPath)
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "HealthCheckPath", healthCheckPath }
        });
        return configurationBuilder.Build();
    }

    private void SetRouteDataValue(string key, object? value)
    {
        var routingFeature = _httpContext.Features.Get<IRoutingFeature>();
        if (routingFeature == null)
        {
            routingFeature = new TestRoutingFeature { RouteData = new RouteData() };
            _httpContext.Features.Set<IRoutingFeature>(routingFeature);
        }
        routingFeature.RouteData.Values[key] = value;
    }

    private class TestRoutingFeature : IRoutingFeature
    {
        public RouteData RouteData { get; set; } = new RouteData();
    }

    [TestMethod]
    public async Task Invoke_WhenHealthCheckPathMatches_ShouldCallNext()
    {
        // Arrange
        var healthCheckPath = "/health";
        _configuration = CreateConfiguration(healthCheckPath);
        _systemUnderTest = new JourneyAccessCheckerMiddleware(_requestDelegateMock.Object, _configuration);
        _httpContext.Request.Path = new PathString("/health/status");

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Once);
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenHealthCheckPathIsEmpty_ShouldProcessNormally()
    {
        // Arrange
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns((Endpoint?)null);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Once);
    }

    [TestMethod]
    public async Task Invoke_WhenNoJourneyAccessAttribute_ShouldCallNext()
    {
        // Arrange
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns((Endpoint?)null);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Once);
        _sessionManagerMock.Verify(x => x.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenApplicationsJourneyType_NoSession_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.Applications);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync((JourneySession?)null);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenApplicationsJourneyType_EmptyJourney_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.Applications);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        var journeySession = new JourneySession
        {
            RegulatorSession = new RegulatorSession { Journey = new List<string>() }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenApplicationsJourneyType_JourneyDoesNotContainPagePath_ShouldRedirectToLastJourneyItem()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.Applications);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        var lastJourneyItem = "previous-page";
        var journeySession = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { "first-page", lastJourneyItem }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{lastJourneyItem}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenApplicationsJourneyType_JourneyContainsPagePath_ShouldCallNext()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.Applications);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        var journeySession = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { "first-page", pagePath, "last-page" }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Once);
        Assert.AreNotEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsStartJourneyType_NoIdInRoute_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissionsStart);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", null);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsStartJourneyType_InvalidIdInRoute_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissionsStart);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", "invalid-guid");

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsStartJourneyType_ValidId_ShouldCallNext()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissionsStart);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        var validId = Guid.NewGuid();
        SetRouteDataValue("id", validId.ToString());

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Once);
        Assert.AreNotEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_NoIdInRoute_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", null);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_NoSession_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var id = Guid.NewGuid();
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", id.ToString());
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync((JourneySession?)null);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_NoPermissionManagementSessionItem_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var id = Guid.NewGuid();
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", id.ToString());

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>()
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_EmptyJourney_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var id = Guid.NewGuid();
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", id.ToString());

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string>()
                    }
                }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_JourneyDoesNotContainPagePath_ShouldRedirectToLastJourneyItem()
    {
        // Arrange
        var pagePath = "test-page";
        var id = Guid.NewGuid();
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", id.ToString());

        var lastJourneyItem = "previous-page";
        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { "first-page", lastJourneyItem }
                    }
                }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{lastJourneyItem}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_JourneyContainsPagePath_ShouldCallNext()
    {
        // Arrange
        var pagePath = "test-page";
        var id = Guid.NewGuid();
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", id.ToString());

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { "first-page", $"{pagePath}/{id}", "last-page" }
                    }
                }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Once);
        Assert.AreNotEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task Invoke_WhenManagePermissionsJourneyType_DifferentIdInSession_ShouldRedirectToApplications()
    {
        // Arrange
        var pagePath = "test-page";
        var id = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.ManagePermissions);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);

        SetRouteDataValue("id", id.ToString());

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = differentId,
                        Journey = new List<string> { "some-page" }
                    }
                }
            }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
        _requestDelegateMock.Verify(x => x(_httpContext), Times.Never);
    }

    [TestMethod]
    public async Task Invoke_WhenPathBaseIsSet_ShouldIncludeInRedirect()
    {
        // Arrange
        var pagePath = "test-page";
        var pathBase = "/base";
        var attribute = new JourneyAccessAttribute(pagePath, JourneyName.Applications);
        var endpoint = CreateEndpointWithAttribute(attribute);
        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);
        _httpContext.Request.PathBase = new PathString(pathBase);

        var journeySession = new JourneySession
        {
            RegulatorSession = new RegulatorSession { Journey = new List<string>() }
        };
        _sessionManagerMock.Setup(x => x.GetSessionAsync(_sessionMock.Object)).ReturnsAsync(journeySession);

        // Act
        await _systemUnderTest.Invoke(_httpContext, _sessionManagerMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
        Assert.AreEqual($"{pathBase}/{PagePath.Applications}", _httpContext.Response.Headers.Location.ToString());
    }

    private static Endpoint CreateEndpointWithAttribute(JourneyAccessAttribute attribute)
    {
        var metadata = new EndpointMetadataCollection(attribute);
        return new Endpoint(_ => Task.CompletedTask, metadata, "TestEndpoint");
    }
}
