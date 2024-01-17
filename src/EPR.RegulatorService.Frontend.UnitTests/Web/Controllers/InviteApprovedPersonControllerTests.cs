using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.InviteNewApprovedPerson;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.InviteNewApprovedPerson;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using EPR.RegulatorService.Frontend.Core.Models;
using Microsoft.AspNetCore.Mvc.Routing;

using ViewResult = Microsoft.AspNetCore.Mvc.ViewResult;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class InviteApprovedPersonControllerTests
{
    private Mock<ISessionManager<JourneySession>> _mockSessionManager;
    private Mock<ILogger<InviteNewApprovedPersonController>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IFacadeService> _mockFacade = null!;
    private InviteNewApprovedPersonController _controller;
    private Mock<HttpContext> _httpContextMock = null!;
    private string _backLink = "Some back link";

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
        _mockLogger = new Mock<ILogger<InviteNewApprovedPersonController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();

        configurationSectionMock
            .Setup(section => section.Value)
            .Returns("/regulators");

        _mockConfiguration
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _mockFacade = new Mock<IFacadeService>();

        var urlHelperMock = new Mock<IUrlHelper>();

        urlHelperMock
            .Setup(x => x.ActionContext)
            .Returns(new ActionContext
            {
                HttpContext = _httpContextMock.Object
            });

        urlHelperMock
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns(_backLink);

        _controller = new InviteNewApprovedPersonController(
            _mockSessionManager.Object, _mockLogger.Object, _mockConfiguration.Object,
            _mockFacade.Object)
        {
            Url = urlHelperMock.Object
        };

        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(false, false)]
    public async Task GivenValidSessionValues_WhenCallingEnterPersonNameGetEndpoint_ThenSetSessionValuesCorrectly(bool? hasANewApprovedPersonBeenNominated, bool expectedHasANewApprovedPersonBeenNominated)
    {
        var externalOrganisationId = Guid.NewGuid();
        var organisationName =$"organisationName{externalOrganisationId}";
        var connExternalId = Guid.NewGuid();
        var userNameToDelete = $"SomeUserNameToDelete{externalOrganisationId}";

        var session = new JourneySession
        {
            AddRemoveApprovedUserSession = new AddRemoveApprovedUserSession
            {
                ExternalOrganisationId = externalOrganisationId,
                OrganisationName = organisationName,
                NominationDecision = hasANewApprovedPersonBeenNominated,
                ConnExternalId = connExternalId,
                UserNameToRemove = userNameToDelete
            }
        };

        _mockSessionManager
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.EnterPersonName() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        session.InviteNewApprovedPersonSession.ExternalOrganisationId.Should().Be(externalOrganisationId);
        session.InviteNewApprovedPersonSession.OrganisationName.Should().Be(organisationName);
        session.InviteNewApprovedPersonSession.HasANewApprovedPersonBeenNominated.Should()
            .Be(expectedHasANewApprovedPersonBeenNominated);
        session.InviteNewApprovedPersonSession.RemovedConnectionExternalId.Should()
            .Be(connExternalId);
        session.InviteNewApprovedPersonSession.UserNameToRemove.Should()
            .Be(userNameToDelete);
        result.ViewData["BackLinkToDisplay"].Should().Be(_backLink);
    }

    [TestMethod]
    public async Task GivenValidModel_WhenCallingEnterPersonNamePostEndpoint_ThenSetRelevantSessionValuesCorrectly()
    {
        var input = new EnterPersonNameModel { FirstName = "firstName", LastName = "lastName" };

        var session = new JourneySession();

        _mockSessionManager
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.EnterPersonName(input) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        session.InviteNewApprovedPersonSession.InvitedPersonFirstname.Should().Be(input.FirstName);
        session.InviteNewApprovedPersonSession.InvitedPersonLastname.Should().Be(input.LastName);
    }

    [TestMethod]
    public async Task GivenInvalidModel_WhenCallingEnterPersonNamePostEndpoint_ThenShouldNotRedirectAndStayOnSameView()
    {
        var input = new EnterPersonNameModel { FirstName = "firstName-invalid", LastName = "lastName-invalid" };

        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string>
                {
                    _backLink
                }
            }
        };

        _mockSessionManager
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        _controller.ModelState.AddModelError("Some error", "some error");

        // Act
        var result = await _controller.EnterPersonName(input) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        result.Model.Should().BeOfType<EnterPersonNameModel>();
        result.ViewData["BackLinkToDisplay"].Should().Be(_backLink);
    }

    [TestMethod]
    public async Task GivenNoModelInput_WhenCallingEnterPersonEmailGetEndpoint_ThenReturnsViewResult()
    {
        // Act
        var result = await _controller.EnterPersonEmail() as ViewResult;

        // Assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task GivenValidModelInput_WhenCallingEnterPersonEmailPostEndpoint_ThenReturnsConfirmationRedirectActionResult()
    {
        var model = new EnterPersonEmailModel { Email = "tester@testing.com" };
        // Act
        var result = await _controller.EnterPersonEmail(model);

        // Assert
        Assert.IsNotNull(result);

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(InviteNewApprovedPersonController.Confirmation));
    }

    [TestMethod]
    public async Task GivenInValidModel_WhenCallingEnterPersonEmailPostEndpoint_ShouldNotRedirectToConfirmation()
    {
        var model = new EnterPersonEmailModel();

        _controller.ModelState.AddModelError("Some error", "Some error");
        // Act
        var result = await _controller.EnterPersonEmail(model);

        // Assert
        Assert.IsNotNull(result);

        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).ViewName.Should().NotBe("Confirmation");
    }

   [TestMethod]
    public async Task GivenValidSessionExists_WhenCallingConfirmationGetEndpoint_ThenReturnsViewResult()
    {
        // Arrange
        var session = new JourneySession
        {
            InviteNewApprovedPersonSession = new InviteNewApprovedPersonSession
            {
                InvitedPersonEmail = "tester@testing.com",
                InvitedPersonFirstname = "Firstname",
                InvitedPersonLastname = "Lastname",
                HasANewApprovedPersonBeenNominated = true,
            },
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string>
                {
                    _backLink
                }
            }
        };

        _mockSessionManager
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.Confirmation() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        result.Model.Should().BeOfType<ConfirmationModel>();
        var confirmationModel = result.Model as ConfirmationModel;

        confirmationModel.InvitedApprovedPersonEmail.Should()
            .Be(session.InviteNewApprovedPersonSession.InvitedPersonEmail);
        confirmationModel.InvitedApprovedPersonFullName.Should()
            .Be(session.InviteNewApprovedPersonSession.InvitedPersonFullName);
        confirmationModel.HasANewApprovedPersonBeenNominated.Should()
            .Be(session.InviteNewApprovedPersonSession.HasANewApprovedPersonBeenNominated);
        result.ViewData["BackLinkToDisplay"].Should().Be(_backLink);
    }

    [TestMethod]
    [DynamicData(nameof(GetExistingConnectionExternalIdToRemoveTestCases))]
    public async Task
        GivenExistingConnectionExternalIdToRemoveIsEmptyInSession_WhenCallingSubmit_ShouldRedirectToAccountPermissionHaveChangedAction(
            Guid? existingConnectionExternalIdToRemove,
            string expectedViewName)
    {
        // Arrange

        var session = new JourneySession
        {
            InviteNewApprovedPersonSession = new InviteNewApprovedPersonSession
            {
                RemovedConnectionExternalId = existingConnectionExternalIdToRemove
            }
        };

        _mockFacade
            .Setup(x => x.AddRemoveApprovedUser(It.IsAny<AddRemoveApprovedUserRequest>()))
            .ReturnsAsync(EndpointResponseStatus.Success);

        _mockSessionManager
            .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.Submit() as ViewResult;

        result.Should().NotBeNull();
        result.ViewName.Should().Be(expectedViewName);
    }

    public static IEnumerable<object[]> GetExistingConnectionExternalIdToRemoveTestCases
    {
        get
        {
            yield return new object[] { Guid.Empty, "EmailSentToNominatedApprovedPerson" };
            yield return new object[] { Guid.NewGuid(), "AccountPermissionHaveChanged" };
            yield return new object[] { null, "EmailSentToNominatedApprovedPerson" };
        }
    }
}