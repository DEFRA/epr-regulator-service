using System.Security.Claims;

using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Home;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Home;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

using Microsoft.Extensions.Configuration;

using Organisation = Common.Authorization.Models.Organisation;
using ServiceRole = Frontend.Core.Enums.ServiceRole;

[TestClass]
public class HomeControllerTests
{
    private Mock<HttpContext> _httpContextMock;
    private Mock<HttpResponse> _httpResponseMock;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IOptions<LandingPageConfig>> _configMock;
    private Mock<IOptions<EprCookieOptions>> _cookieConfig;
    private Mock<IResponseCookies> _responseCookiesMock;
    private Mock<ISession> _sessionMock;
    private Mock<IFeatureManager> _mockFeatureManager;
    private HomeController _systemUnderTest;
    private JourneySession _journeySession;
    private UserData _userData;

    private const string FirstName = "John";
    private const string LastName = "Smith";
    private const string OrganisationName = "ACME";
    private const string ManageAccountUrl = "/manage-account/manage";
    private const string ApplicationsUrl = "/regulators/applications";
    private const string SessionCookieName = "SessionCookieName";
    private const string ManageRegistraionSubmissionsUrl = "/manage-registration-submissions";

    public void Setup(
        ServiceRole serviceRole,
        LandingPageConfig landingPageConfig = null,
        bool getUserDataFromClaimsPrinciple = false)
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _responseCookiesMock = new Mock<IResponseCookies>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _configMock = new Mock<IOptions<LandingPageConfig>>();
        _cookieConfig = new Mock<IOptions<EprCookieOptions>>();
        _sessionMock = new Mock<ISession>();
        _mockFeatureManager = new Mock<IFeatureManager>();

        _userData = new UserData()
        {
            FirstName = FirstName,
            LastName = LastName,
            Organisations = new List<Organisation> { new() { Name = OrganisationName } },
            ServiceRoleId = (int)serviceRole
        };

        if (getUserDataFromClaimsPrinciple)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            claimsPrincipal.AddOrUpdateUserData(_userData);

            _httpContextMock.Setup(mock => mock.User).Returns(claimsPrincipal);
        }
        else
        {
            _journeySession = new JourneySession { UserData = _userData };

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);
        }

        if (landingPageConfig != null)
        {
            _configMock.Setup(mock => mock.Value).Returns(landingPageConfig);
        }
        else
        {
            _configMock.Setup(mock => mock.Value).Returns(new LandingPageConfig
            {
                ManageAccountUrl = ManageAccountUrl,
                ApplicationsUrl = ApplicationsUrl
            });
        }

        _cookieConfig.Setup(m => m.Value).Returns(new EprCookieOptions { SessionCookieName = "SessionCookieName" });
        _httpContextMock.Setup(m => m.Response).Returns(_httpResponseMock.Object);
        _httpContextMock.Setup(m => m.Session).Returns(_sessionMock.Object);
        _httpResponseMock.Setup(m => m.Cookies).Returns(_responseCookiesMock.Object);

        var configurationMock = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationMock.Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);
        configurationSectionMock.Setup(section => section.Value).Returns("/path/base");

        _systemUnderTest = new HomeController(
            _sessionManagerMock.Object,
            _mockFeatureManager.Object,
            _configMock.Object,
            _cookieConfig.Object,
            configurationMock.Object);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task
        GivenOnLandingPage_WhenUserHasValidServiceRole_ThenShouldShowLandingPage()
    {
        // Arrange
        Setup(ServiceRole.RegulatorAdmin);

        // Act
        var result = (await _systemUnderTest.LandingPage()) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        result!.ViewName.Should().Be("LandingPage");

        _httpContextMock.Verify(x => x.User, Times.Never);
    }

    [TestMethod]
    public async Task GivenOnLandingPage_ThenJourneyShouldOnlyContainLandingPage()
    {
        // Arrange
        Setup(ServiceRole.RegulatorAdmin);

        // Act
        await _systemUnderTest.LandingPage();

        // Assert
        _httpContextMock.Verify(x => x.User, Times.Never);

        _journeySession.RegulatorSession.Journey.Should().HaveCount(1);
        _journeySession.RegulatorSession.Journey[0].Should().Be(PagePath.Home);
    }

    [TestMethod]
    public async Task GivenOnLandingPage_WhenUserHasValidServiceRole_ThenShouldHaveCorrectUrlsInViewModel()
    {
        // Arrange
        Setup(ServiceRole.RegulatorAdmin);

        // Act
        var result = (await _systemUnderTest.LandingPage()) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Model);
        result.Model.Should().BeOfType<LandingPageViewModel>();

        var viewmodel = (LandingPageViewModel)result.Model;
        viewmodel.ManageAccountUrl.Should().Be(ManageAccountUrl);
        viewmodel.ApplicationsUrl.Should().Be(ApplicationsUrl);

        _httpContextMock.Verify(x => x.User, Times.Never);
    }

    [TestMethod]
    public async Task GivenOnLandingPage_WhenUserHasValidServiceRole_ThenShouldHaveCorrectUserDataInViewModel()
    {
        // Arrange
        Setup(ServiceRole.RegulatorAdmin);

        // Act
        var result = (await _systemUnderTest.LandingPage()) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Model);
        result.Model.Should().BeOfType<LandingPageViewModel>();

        var viewModel = (LandingPageViewModel)result.Model;
        viewModel.OrganisationName.Should().Be(OrganisationName);
        viewModel.PersonName.Should().Be($"{FirstName} {LastName}");

        _httpContextMock.Verify(x => x.User, Times.Never);
    }

    [TestMethod]
    public async Task
        GivenOnLandingPage_AndTheUserIsAnAdmin_ThenShouldHaveIsAdminTrueInViewModel()
    {
        // Arrange
        Setup(ServiceRole.RegulatorAdmin);

        // Act
        var result = (await _systemUnderTest.LandingPage()) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result.Model.Should().BeOfType<LandingPageViewModel>();

        var viewModel = (LandingPageViewModel)result.Model!;
        viewModel.IsRegulatorAdmin.Should().BeTrue();

        _httpContextMock.Verify(x => x.User, Times.Never);
    }

    [TestMethod]
    public async Task
        GivenOnLandingPage_AndTheUserIsABasicUser_ThenShouldHaveIsAdminFalseInViewModel()
    {
        // Arrange
        Setup(ServiceRole.RegulatorBasic);

        // Act
        var result = (await _systemUnderTest.LandingPage()) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().NotBeNull();
        result.Model.Should().BeOfType<LandingPageViewModel>();

        var viewModel = (LandingPageViewModel)result.Model!;
        viewModel.IsRegulatorAdmin.Should().BeFalse();

        _httpContextMock.Verify(x => x.User, Times.Never);
    }

    [TestMethod]
    public async Task
        GivenOnLandingPage_AndTheUserIsABasicUser_ThenShouldHaveValidUserData()
    {
        // Arrange
        Setup(ServiceRole.RegulatorBasic, null, true);

        // Act
        var result = (await _systemUnderTest.LandingPage()) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Model);
        result.Model.Should().BeOfType<LandingPageViewModel>();

        var viewModel = (LandingPageViewModel)result.Model;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(expected: viewModel.OrganisationName, actual: _userData.Organisations[0].Name);
        Assert.AreEqual(expected: viewModel.PersonName, actual: $"{_userData.FirstName} {_userData.LastName}");
        Assert.IsFalse(viewModel.IsRegulatorAdmin);

        _httpContextMock.Verify(x => x.User, Times.Once);
    }

    [TestMethod]
    public void OnSignOut_DeleteUserSessionCookie()
    {
        // Arrange
        Setup(ServiceRole.RegulatorBasic, null, true);

        // Act
        _systemUnderTest.SignedOut();

        // Assert
        _responseCookiesMock.Verify(x => x.Delete(SessionCookieName), Times.Once);
    }

    [TestMethod]
    public async Task LandingPage_ShouldIncludeManageRegistrationSubmissionsUrl_WhenManageRegistrationSubmissionsFeatureFlagIsEnabled()
    {
        // Arrange
        var landingPageConfig = new LandingPageConfig
        {
            ManageRegistrationSubmissionsUrl = ManageRegistraionSubmissionsUrl, // Expected URL when feature flag is enabled
            ManageAccountUrl = ManageAccountUrl,
            ApplicationsUrl = ApplicationsUrl
        };

        Setup(ServiceRole.RegulatorAdmin, landingPageConfig);

        _mockFeatureManager.Setup(fm =>
            fm.IsEnabledAsync(FeatureFlags.ManageRegistrationSubmissions))
            .ReturnsAsync(true); // Mock feature flag enabled

        // Act
        var result = await _systemUnderTest.LandingPage() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var viewModel = result.Model as LandingPageViewModel;
        Assert.IsNotNull(viewModel);

        // Verify the URL is included when the feature flag is enabled
        Assert.AreEqual(ManageRegistraionSubmissionsUrl, viewModel.ManageRegistrationSubmissionsUrl);
        Assert.AreEqual(ManageAccountUrl, viewModel.ManageAccountUrl);
        Assert.AreEqual(ApplicationsUrl, viewModel.ApplicationsUrl);

        _httpContextMock.Verify(x => x.User, Times.Never);
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task LandingPage_ShouldNotIncludeManageRegistrationSubmissionsUrl_WhenManageRegistrationSubmissionsFeatureFlagIsDisabled()
    {
        // Arrange
        var landingPageConfig = new LandingPageConfig
        {
            ManageRegistrationSubmissionsUrl = string.Empty,
            ManageAccountUrl = ManageAccountUrl,
            ApplicationsUrl = ApplicationsUrl
        };

        Setup(ServiceRole.RegulatorAdmin, landingPageConfig);

        _mockFeatureManager.Setup(fm =>
            fm.IsEnabledAsync(FeatureFlags.ManageRegistrationSubmissions))
            .ReturnsAsync(false); // Mock feature flag disabled

        // Act
        var result = await _systemUnderTest.LandingPage() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var viewModel = result.Model as LandingPageViewModel;
        Assert.IsNotNull(viewModel);

        // Verify the URL is NOT included when the feature flag is disabled
        Assert.AreEqual(string.Empty, viewModel.ManageRegistrationSubmissionsUrl);
        Assert.AreEqual(ManageAccountUrl, viewModel.ManageAccountUrl);
        Assert.AreEqual(ApplicationsUrl, viewModel.ApplicationsUrl);

        _httpContextMock.Verify(x => x.User, Times.Never);
        _mockFeatureManager.Verify(fm => fm.IsEnabledAsync(It.IsAny<string>()), Times.Once);
    }
}