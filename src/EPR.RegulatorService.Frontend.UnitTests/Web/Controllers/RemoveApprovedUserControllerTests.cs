using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.RemoveApprovedUser;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class RemoveApprovedUserControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private Mock<ISessionManager<JourneySession>> _mockSessionManager;
    private Mock<ILogger<RemoveApprovedUserController>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    private RemoveApprovedUserController _controller;
    private Mock<HttpContext> _httpContextMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
        _mockLogger = new Mock<ILogger<RemoveApprovedUserController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(section => section.Value).Returns("/regulators");
        _mockConfiguration.Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _controller = new RemoveApprovedUserController(_mockSessionManager.Object, _mockLogger.Object, _mockConfiguration.Object);
        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    private static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        bool hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Contain(expectedBackLink);
    }

    [TestMethod]
    public async Task Confirm_ReturnsViewResultAndSessionData_WhenSessionIsValid()
    {
        // Arrange
        string userNameToDelete = "testUser";
        string organisationName = "testOrg";
        Guid connExternalId = new();
        Guid organisationId = new();
        var session = new JourneySession();
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.Confirm(userNameToDelete, organisationName, connExternalId,organisationId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        session.RemoveApprovedUserSession.OrganisationName.Should().Be(organisationName);
        session.RemoveApprovedUserSession.UserNameToDelete.Should().Be(userNameToDelete);
        session.RemoveApprovedUserSession.ConnExternalId.Should().Be(connExternalId);
        session.RemoveApprovedUserSession.OrganisationId.Should().Be(organisationId);
        AssertBackLink(result, PagePath.Home);
    }

    [TestMethod]
    public async Task NominationDecision_ReturnsViewResult_WhenSessionIsValid()
    {
        // Arrange
        var session = new JourneySession();
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.NominationDecision() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        AssertBackLink(result, "confirm-remove-user");
    }

    [TestMethod]
    public async Task Nomination_ReturnsRedirectToAction_WhenNominationDecisionIsTrue()
    {
        // Arrange
        var session = new JourneySession();
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.Nomination(true) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(session.RemoveApprovedUserSession.NominationDecision);
    }
    [TestMethod]
    public async Task Nomination_ReturnsRedirectToAction_WhenNominationDecisionIsFalse()
    {
        // Arrange
        var session = new JourneySession();
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.Nomination(false) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(session.RemoveApprovedUserSession.NominationDecision);
    }

    [TestMethod]
    public async Task Nomination_ReturnsView_WhenNominationDecisionIsNull()
    {
        // Arrange
        var session = new JourneySession();
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.Nomination(null) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        AssertBackLink(result, PagePath.NominationDecision);
        Assert.IsTrue(result.ViewData.ModelState.ContainsKey(nameof(session.RemoveApprovedUserSession.NominationDecision)));
        Assert.IsNull(session.RemoveApprovedUserSession.NominationDecision);
    }

    [TestMethod]
    public async Task ConfirmNominationDecision_ReturnsViewResult_WhenSessionIsValid()
    {
        // Arrange
        var session = new JourneySession();
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.ConfirmNominationDecision() as ViewResult;

        // Assert
        AssertBackLink(result, PagePath.NominationDecision);
    }
}