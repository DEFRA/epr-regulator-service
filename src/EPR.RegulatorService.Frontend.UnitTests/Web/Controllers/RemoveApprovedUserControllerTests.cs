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

using Frontend.Web.ViewModels.ApprovedPersonListPage;

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

    private OrganisationUser _userJohn;
    private OrganisationUser _userJames;

    [TestInitialize]
    public void Setup()
    {
        _userJames = new OrganisationUser { FirstName = "James", LastName = "Smith", Email = "jsmith@gmail.com", PersonExternalId = Guid.NewGuid() };
        _userJohn = new OrganisationUser { FirstName = "John", LastName = "Lewis", Email = "jlewis@gmail.com", PersonExternalId = Guid.NewGuid() };

        _httpContextMock = new Mock<HttpContext>();
        _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
        _mockLogger = new Mock<ILogger<RemoveApprovedUserController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(section => section.Value).Returns("/regulators");
        _mockConfiguration.Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);
        _mockFacade = new Mock<IFacadeService>();

        _mockFacade
            .Setup(x => x.GetProducerOrganisationUsersByOrganisationExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationUser>() { _userJohn, _userJames });

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
        Guid connExternalId = Guid.NewGuid();
        Guid organisationId = Guid.NewGuid();
        var session = new JourneySession();
        session.RegulatorSession.Journey.Add("home");
        session.RegulatorSession.Journey.Add("confirm-remove-user");
        _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        var result = await _controller.Confirm(userNameToDelete, organisationName, connExternalId,organisationId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        session.AddRemoveApprovedUserSession.OrganisationName.Should().Be(organisationName);
        session.AddRemoveApprovedUserSession.UserNameToDelete.Should().Be(userNameToDelete);
        session.AddRemoveApprovedUserSession.ConnExternalId.Should().Be(connExternalId);
        session.AddRemoveApprovedUserSession.OrganisationId.Should().Be(organisationId);
        AssertBackLink(result, PagePath.Home);
    }

    [TestMethod]
    public async Task NominationDecision_ReturnsViewResult_WhenSessionIsValid()
    {
        // Arrange
        var session = new JourneySession();
        session.RegulatorSession.Journey.Add("confirm-remove-user");
        session.RegulatorSession.Journey.Add("approve-confirmation");
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
        Assert.IsTrue(session.AddRemoveApprovedUserSession.NominationDecision);
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
        Assert.IsFalse(session.AddRemoveApprovedUserSession.NominationDecision);
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
        Assert.IsTrue(result.ViewData.ModelState.ContainsKey(nameof(session.AddRemoveApprovedUserSession.NominationDecision)));
        Assert.IsNull(session.AddRemoveApprovedUserSession.NominationDecision);
    }

    [TestMethod]
    public async Task ConfirmNominationDecision_ReturnsViewResult_WhenSessionIsValid()
    {
        // Arrange
        var session = new JourneySession();
        session.RegulatorSession.Journey.Add("approve-confirmation");
        session.RegulatorSession.Journey.Add("approve-decision-confirmation");

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

       var resultModel = result.Model as AddRemoveApprovedUserSession;
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


    [TestMethod]
    public async Task ApprovedPersonListPage_ReturnsViewWithModel()
    {
        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { PagePath.RegulatorCompanyDetail, PagePath.ApprovedPersonListPage }
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var orgId = Guid.NewGuid();

        var result = await _controller.ApprovedPersonListPage(orgId) as ViewResult;
        Assert.IsNotNull(result);
        result.Model.Should().BeOfType<OrganisationUsersModel>();

        ValidateOrganisationUsersModel(result, orgId);

        result.ViewData.Should().ContainKey("BackLinkToDisplay");
        result.ViewData["BackLinkToDisplay"].Should().Be(PagePath.RegulatorCompanyDetail);
    }

    [TestMethod]
    public async Task ApprovedPersonListPage_ValidModel_RedirectsToNextPage()
    {
        var session = new JourneySession();

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var input = new OrganisationUsersModel
        {
            NewApprovedUserId = Guid.NewGuid(),
            ExternalOrganisationId = Guid.NewGuid()
        };

        await _controller.ApprovedPersonListPage(input);

        _mockSessionManager.Verify(x =>
                x.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }


    [TestMethod]
    public async Task ApprovedPersonListPage_WithNo_NewApprovedUserId_ReturnsSamePage()
    {
        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { PagePath.RegulatorCompanyDetail, PagePath.ApprovedPersonListPage }
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var input = new OrganisationUsersModel
        {
            ExternalOrganisationId = Guid.NewGuid()
        };

        // Manually add an error to ModelState
        _controller.ModelState.AddModelError("NewApprovedUserId", "Error message");

        var result = await _controller.ApprovedPersonListPage(input) as ViewResult;

        ValidateOrganisationUsersModel(result, input.ExternalOrganisationId);

        AssertBackLink(result, PagePath.RegulatorCompanyDetail);

        _mockSessionManager.Verify(x =>
            x.SaveSessionAsync(It.IsAny<ISession>(), session), Times.Once);
    }


    [TestMethod]
    public async Task ApprovedPersonListPage_Stores_ExternalId_To_Session()
    {
        // arrange
        var externalId = Guid.NewGuid();
        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { PagePath.RegulatorCompanyDetail, PagePath.ApprovedPersonListPage }
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // act
        await _controller.ApprovedPersonListPage(externalId);

        // assert
        session.AddRemoveApprovedUserSession.OrganisationId.Should().Be(externalId);
    }



    private void ValidateOrganisationUsersModel(ViewResult result, Guid orgId)
    {
        var model = result.Model as OrganisationUsersModel;
        model.ExternalOrganisationId.Should().Be(orgId);
        model.OrganisationUsers.Count.Should().Be(2);
        model.OrganisationUsers[0].FirstName.Should().Be(_userJohn.FirstName);
        model.OrganisationUsers[0].LastName.Should().Be(_userJohn.LastName);
        model.OrganisationUsers[0].Email.Should().Be(_userJohn.Email);
        model.OrganisationUsers[0].PersonExternalId.Should().Be(_userJohn.PersonExternalId);
        model.OrganisationUsers[1].FirstName.Should().Be(_userJames.FirstName);
        model.OrganisationUsers[1].LastName.Should().Be(_userJames.LastName);
        model.OrganisationUsers[1].Email.Should().Be(_userJames.Email);
        model.OrganisationUsers[1].PersonExternalId.Should().Be(_userJames.PersonExternalId);

    }
}