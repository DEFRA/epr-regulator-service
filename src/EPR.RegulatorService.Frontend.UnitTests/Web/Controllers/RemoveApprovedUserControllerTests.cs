using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.RemoveApprovedUser;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.ViewModels.RemoveApprovedUser;
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
    private Mock<IFacadeService> _mockFacade = null!;
    private RemoveApprovedUserController _controller;
    private Mock<HttpContext> _httpContextMock = null!;
    private readonly Guid _organisationExternalId = Guid.NewGuid();
    private readonly Guid _connExternalId = Guid.NewGuid();

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
        _mockFacade = new Mock<IFacadeService>();

        _controller = new RemoveApprovedUserController(_mockSessionManager.Object, _mockLogger.Object, _mockConfiguration.Object, _mockFacade.Object);
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
        var result = await _controller.Nomination(new ApprovedUserToRemoveViewModel{ NominationDecision = true }) as RedirectToActionResult;

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
        var result = await _controller.Nomination(new ApprovedUserToRemoveViewModel{NominationDecision = false }) as RedirectToActionResult;

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
        var result = await _controller.Nomination(new ApprovedUserToRemoveViewModel{NominationDecision = null }) as ViewResult;

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

    [TestMethod]
    public async Task Submit_Returns_RemovedConfirmation_When_ApprovedUser_Response_Returns_Success()
    {
        // Arrange
        var model = new ApprovedUserToRemoveViewModel
        {
            ConnExternalId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            NominationDecision = false
        };

        var session = new JourneySession
        {
            RegulatorSession =
                new RegulatorSession
                {
                    OrganisationId = model.OrganisationId,
                    ConnExternalId = model.ConnExternalId,
                    NominationDecision = model.NominationDecision
                }
        };
        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);


        _mockFacade
            .Setup(x => x.RemoveApprovedUser(It.IsAny<RemoveApprovedUserRequest>()))
            .ReturnsAsync(EndpointResponseStatus.Success);

        // Act
        var result = await _controller.Submit(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        result.ViewName.Should().Be("RemovedConfirmation");

       var resultModel = result.Model as RemoveApprovedUserSession;
       Assert.IsNotNull(resultModel);
       resultModel.ResponseStatus.Should().Be(EndpointResponseStatus.Success);
    }

    [TestMethod]
    public async Task Submit_Returns_Error_When_ApprovedUser_Response_Returns_Fail()
    {
        // Arrange

       var session = new JourneySession
        {
            RegulatorSession =
                new RegulatorSession
                {
                    OrganisationId = _organisationExternalId,
                    ConnExternalId = _connExternalId,
                    NominationDecision = false
                }
        };
        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _mockFacade
            .Setup(x => x.RemoveApprovedUser(new RemoveApprovedUserRequest()))
            .ReturnsAsync(EndpointResponseStatus.Fail);

        var vm = new ApprovedUserToRemoveViewModel
        {
            OrganisationId = Guid.NewGuid(),
            ConnExternalId = Guid.NewGuid(),
            NominationDecision = false
        };
        // Act
        var result = await _controller.Submit(vm) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ActionName);
        result.ControllerName.Should().Be("Error");
        result.ActionName.Should().Be("error");

    }
}