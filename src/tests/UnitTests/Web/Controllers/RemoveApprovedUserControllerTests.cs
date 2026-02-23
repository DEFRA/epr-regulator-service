using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.RemoveApprovedUser;
using EPR.RegulatorService.Frontend.Web.Controllers.InviteNewApprovedPerson;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.ViewModels.RemoveApprovedUser;
using EPR.RegulatorService.Frontend.Web.ViewModels.ApprovedPersonListPage;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class RemoveApprovedUserControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private Mock<ISessionManager<JourneySession>> _mockSessionManager;
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
        _mockConfiguration = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(section => section.Value).Returns("/regulators");
        _mockConfiguration.Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);
        _mockFacade = new Mock<IFacadeService>();

        _mockFacade
            .Setup(x => x.GetProducerOrganisationUsersByOrganisationExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationUser>() { _userJohn, _userJames });

        _controller = new RemoveApprovedUserController(_mockSessionManager.Object, _mockConfiguration.Object, _mockFacade.Object);
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
        session.AddRemoveApprovedUserSession.UserNameToRemove.Should().Be(userNameToDelete);
        session.AddRemoveApprovedUserSession.ConnExternalId.Should().Be(connExternalId);
        session.AddRemoveApprovedUserSession.ExternalOrganisationId.Should().Be(organisationId);
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
            NominationDecision = false,
            PromotedPersonExternalId = Guid.NewGuid()
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
    public async Task Submit_Returns_NominatedConfirmation_When_Response_Returns_Success()
    {
        // Arrange
        var model = new ApprovedUserToRemoveViewModel
        {
            ConnExternalId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            PromotedPersonExternalId = Guid.NewGuid(),
            NominationDecision = null
        };

        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                OrganisationId = model.OrganisationId,
                ConnExternalId = model.ConnExternalId,
                NominationDecision = model.NominationDecision
            },
            AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession()
            {
                NewApprovedUser = new OrganisationUser()
                {
                    FirstName = "Joe",
                    LastName = "Bloggs"
                }
            }
        };
        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);


        _mockFacade
            .Setup(x => x.RemoveApprovedUser(It.IsAny<RemoveApprovedUserRequest>()))
            .ReturnsAsync(EndpointResponseStatus.Success);

        // Act
        var result = await _controller.Submit(model)  as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        result.ActionName.Should().Be("EmailNominatedApprovedPerson");
    }

    [TestMethod]
    public async Task NominateOnly_Returns_RedirectToAction_With_CorrectModel()
    {
        // Arrange
        var addRemoveApprovedUserSession = new AddRemoveApprovedUserSession
        {
            NewApprovedUser = new OrganisationUser
            {
                FirstName = "John",
                LastName = "Doe"
            },
            OrganisationName = "SampleOrg"
        };


        // Act
        var result = await _controller.NominateOnly(addRemoveApprovedUserSession) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("EmailNominatedApprovedPerson", result.ActionName);
               
        result.RouteValues.Should().ContainKey("PromotedUserName");
        result.RouteValues.Should().ContainKey("OrganisationName");
        var promotedUserName = result.RouteValues["PromotedUserName"] as string;
        var organisationName = result.RouteValues["OrganisationName"] as string;
        Assert.AreEqual("John Doe", promotedUserName);
        Assert.AreEqual("SampleOrg", organisationName);
        

    }

    [TestMethod]
    public async Task Submit_Returns_RemoveAndNominatedConfirmation_When_Response_Returns_Success()
    {
        // Arrange
        var model = new ApprovedUserToRemoveViewModel
        {
            ConnExternalId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            PromotedPersonExternalId = Guid.NewGuid(),
            NominationDecision = true
        };

        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                OrganisationId = model.OrganisationId,
                ConnExternalId = model.ConnExternalId,
                NominationDecision = model.NominationDecision
            },
            AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession()
            {
                NewApprovedUser = new OrganisationUser()
                {
                    FirstName = "Joe",
                    LastName = "Bloggs"
                },
                OrganisationName = "Acme Ltd",
                UserNameToRemove = "Jenny Smith"
            }
        };
        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);


        _mockFacade
            .Setup(x => x.RemoveApprovedUser(It.IsAny<RemoveApprovedUserRequest>()))
            .ReturnsAsync(EndpointResponseStatus.Success);

        // Act
        var result = await _controller.Submit(model)  as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        result.ActionName.Should().Be("AccountPermissionsChanged");
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
    public async Task Submit_ValidRequest_Should_Save_To_Session_Correctly()
    {
        // Arrange
        var model = new ApprovedUserToRemoveViewModel
        {
            ConnExternalId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            NominationDecision = false
        };
        var session = new JourneySession();

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);


        _mockFacade
            .Setup(x => x.RemoveApprovedUser(It.IsAny<RemoveApprovedUserRequest>()))
            .ReturnsAsync(EndpointResponseStatus.Success);

        // Act
        var result = await _controller.Submit(model) as ViewResult;

        // Assert
        var resultModel = result.Model as AddRemoveApprovedUserSession;
        Assert.IsNotNull(resultModel);
        resultModel.ConnExternalId.Should().Be(model.ConnExternalId);
        resultModel.ExternalOrganisationId.Should().Be(model.OrganisationId);
        resultModel.NominationDecision.Should().Be(model.NominationDecision);

        session.AddRemoveApprovedUserSession.ConnExternalId.Should().Be(model.ConnExternalId);
        session.AddRemoveApprovedUserSession.ExternalOrganisationId.Should().Be(model.OrganisationId);
        session.AddRemoveApprovedUserSession.NominationDecision.Should().Be(model.NominationDecision);
    }

    [TestMethod]
    public async Task ApprovedPersonListPage_ReturnsViewWithModel()
    {
        var orgId = Guid.NewGuid();
        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { PagePath.RegulatorCompanyDetail, PagePath.ApprovedPersonListPage }
            },
            AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
            {
                ExternalOrganisationId = orgId
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);



        var result = await _controller.ApprovedPersonListPage() as ViewResult;
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
            },
            AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
            {
                ExternalOrganisationId = externalId
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // act
        await _controller.ApprovedPersonListPage();

        // assert
        session.AddRemoveApprovedUserSession.ExternalOrganisationId.Should().Be(externalId);
    }

    [TestMethod]
    public async Task NewApprovedUserIdIsEmpty_ApprovedPersonListPage_RedirectToEnterPersonName()
    {
        // arrange
        var input = new OrganisationUsersModel
        {
            NewApprovedUserId = Guid.Empty,
            ExternalOrganisationId = Guid.NewGuid()
        };

        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { PagePath.RegulatorCompanyDetail, PagePath.ApprovedPersonListPage }
            },
            AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
            {
                ExternalOrganisationId = input.ExternalOrganisationId
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // act
        var result = await _controller.ApprovedPersonListPage(input) as RedirectToActionResult;

        // assert
        result.Should().NotBeNull();
        result.ActionName.Should().Be(nameof(InviteNewApprovedPersonController.EnterPersonName));
        session.AddRemoveApprovedUserSession.ExternalOrganisationId.Should().Be(input.ExternalOrganisationId);
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