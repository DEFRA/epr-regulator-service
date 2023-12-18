using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.Controllers.SearchManageApprovers;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorSearchPage;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.ViewModels.ApprovedPersonListPage;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class SearchManagedApproversTests
{
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<HttpContext> _httpContextMock = null!;
    private SearchManageApproversController _systemUnderTest = null!;
    private Mock<ISessionManager<JourneySession>> _mockSessionManager;
    private Mock<ILogger<SearchManageApproversController>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    private readonly string _pathBase = "/path/base";
    private readonly int _pageSize = 20;
    private readonly Guid _organisationId = Guid.NewGuid();

    private OrganisationUser _userJohn;
    private OrganisationUser _userJames;

    private void SetupBase()
    {
        _userJames = new OrganisationUser { FirstName = "James", LastName = "Smith", Email = "jsmith@gmail.com", PersonExternalId = Guid.NewGuid() };
        _userJohn = new OrganisationUser { FirstName = "John", LastName = "Lewis", Email = "jlewis@gmail.com", PersonExternalId = Guid.NewGuid() };

        _httpContextMock = new Mock<HttpContext>();
        var mockFeatureManager = new Mock<IFeatureManager>();
        mockFeatureManager.Setup(f => f.IsEnabledAsync(FeatureFlags.ManageApprovedUsers))
            .Returns(Task.FromResult(false));

        _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
        _mockLogger = new Mock<ILogger<SearchManageApproversController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(section => section.Value).Returns(_pathBase);
        _mockConfiguration.Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _facadeServiceMock = new Mock<IFacadeService>();
        _facadeServiceMock
            .Setup(x => x.GetOrganisationBySearchTerm(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new PaginatedList<OrganisationSearchResult>
            {
                Items = new List<OrganisationSearchResult>(),
                PageSize = _pageSize
            });

        _facadeServiceMock
            .Setup(x => x.GetRegulatorCompanyDetails(It.IsAny<Guid>()))
            .ReturnsAsync(MockedOrganisationDetails.GetMockedOrganisationDetails);

        _facadeServiceMock
            .Setup(x => x.GetProducerOrganisationUsersByOrganisationExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new List<OrganisationUser>() { _userJohn, _userJames });

        _systemUnderTest = new SearchManageApproversController(
            _mockSessionManager.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _facadeServiceMock.Object);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestInitialize]
    public void TestInit() => SetupBase();

    [TestMethod]
    public async Task RegulatorSearchPage_NullSessionManager_ExpectNullArgumentException()
    {
        var mockLogger = new Mock<ILogger<SearchManageApproversController>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockFacade = new Mock<IFacadeService>();
        Assert.ThrowsException<ArgumentNullException>(() =>
            new SearchManageApproversController(null, mockLogger.Object, mockConfiguration.Object, mockFacade.Object));
    }

    [TestMethod]
    public async Task RegulatorSearchPage_NullLogger_ExpectNullArgumentException()
    {
        var sessionManager = new Mock<ISessionManager<JourneySession>>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockFacade = new Mock<IFacadeService>();
        Assert.ThrowsException<ArgumentNullException>(() =>
            new SearchManageApproversController(sessionManager.Object, null, mockConfiguration.Object, mockFacade.Object));
    }

    [TestMethod]
    public async Task RegulatorSearchPage_NullConfig_ExpectNullArgumentException()
    {
        var sessionManager = new Mock<ISessionManager<JourneySession>>();
        var mockLogger = new Mock<ILogger<SearchManageApproversController>>();
        var mockFacade = new Mock<IFacadeService>();
        Assert.ThrowsException<ArgumentNullException>(() =>
            new SearchManageApproversController(sessionManager.Object, mockLogger.Object, null, mockFacade.Object));
    }

    [TestMethod]
    public async Task RegulatorSearchPage_ReturnsViewWithModel()
    {
        var result = await _systemUnderTest.RegulatorSearchPage() as ViewResult;
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Model, typeof(SearchTermViewModel));
    }

    [TestMethod]
    public async Task RegulatorSearchPage_SaveSearchTermInSession()
    {
        var searchTerm = "some search term";
        var input = new SearchTermViewModel { SearchTerm = searchTerm };

        _ = await _systemUnderTest.RegulatorSearchPage(input) as ViewResult;

        _mockSessionManager.Verify(x =>
            x.SaveSessionAsync(
                It.IsAny<ISession>(),
                It.Is<JourneySession>(journeySession => journeySession.SearchManageApproversSession.SearchTerm == searchTerm)),
            Times.Once);
    }

    [TestMethod]
    [DataRow("some search term", 100, "some search term", 100)]
    [DataRow("some search term", null, "some search term", 1)]
    public async Task RegulatorSearchResult_WithNullSession_ReturnsViewWithModel(
        string searchTerm, int? pageNumber,
        string expectedSearchTerm, int expectedPageNumber)
    {
        var session = new JourneySession
        {
            RegulatorSession = new RegulatorSession
            {
                Journey = new List<string> { PagePath.RegulatorSearchPage, PagePath.RegulatorSearchResult }
            },
            SearchManageApproversSession = new SearchManageApproversSession { SearchTerm = searchTerm }
        };
        _mockSessionManager
            .Setup(sm =>sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var result = await _systemUnderTest.RegulatorSearchResult(pageNumber) as ViewResult;
        Assert.IsNotNull(result);
        result.Model.Should().BeOfType<OrganisationSearchResultsListViewModel>();

        var model = result.Model as OrganisationSearchResultsListViewModel;
        model.OrganisationSearchFilterModel.SearchTerm.Should().Be(expectedSearchTerm);
        session.SearchManageApproversSession.CurrentPageNumber.Should().Be(expectedPageNumber);

        result.ViewData.Should().ContainKey("BackLinkToDisplay");
        result.ViewData["BackLinkToDisplay"].Should().Be(PagePath.RegulatorSearchPage);
    }

    [TestMethod]
    [DataRow(null, "searchTerm", 1, "searchTerm", 1)]
    [DataRow(100, "searchTerm", 1, "searchTerm", 100)]
    public async Task RegulatorSearchResult_WithVariousComboOfInputParametersAndSessionValues_ReturnsViewWithModel(
        int? pageNumber, string searchTermFromSession,
        int? pageNumberFromSession, string expectedSearchTerm,
        int? expectedPageNumber)
    {
        var session = new JourneySession
        {
            RegulatorSession =
                new RegulatorSession
                {
                    Journey = new List<string> { PagePath.RegulatorSearchPage, PagePath.RegulatorSearchResult }
                },
            SearchManageApproversSession = new SearchManageApproversSession
            {
                SearchTerm = searchTermFromSession, CurrentPageNumber = pageNumberFromSession
            }
        };

        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        var result = await _systemUnderTest.RegulatorSearchResult(pageNumber) as ViewResult;

        Assert.IsNotNull(result);
        result.Model.Should().BeOfType<OrganisationSearchResultsListViewModel>();

        var model = result.Model as OrganisationSearchResultsListViewModel;
        model.OrganisationSearchFilterModel.SearchTerm.Should().Be(expectedSearchTerm);
        session.SearchManageApproversSession.CurrentPageNumber.Should().Be(expectedPageNumber);

        result.ViewData.Should().ContainKey("BackLinkToDisplay");
        result.ViewData["BackLinkToDisplay"].Should().Be(PagePath.RegulatorSearchPage);
    }


    [TestMethod]
    public async Task RegulatorCompanyDetails_ReturnsViewWithModel()
    {
        var session = new JourneySession
        {
            RegulatorSession =
                new RegulatorSession
                {
                    Journey = new List<string> { PagePath.RegulatorSearchPage, PagePath.RegulatorSearchResult, PagePath.RegulatorCompanyDetail },
                    OrganisationId = _organisationId
                }
        };
        _mockSessionManager
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);


        var result = await _systemUnderTest.RegulatorCompanyDetail(_organisationId) as ViewResult;
        Assert.IsNotNull(result);
        result.Model.Should().BeOfType<RegulatorCompanyDetailViewModel>();

        result.ViewData.Should().ContainKey("BackLinkToDisplay");
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

        var result = await _systemUnderTest.ApprovedPersonListPage(orgId) as ViewResult;
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

        await _systemUnderTest.ApprovedPersonListPage(input);

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
        _systemUnderTest.ModelState.AddModelError("NewApprovedUserId", "Error message");

        var result = await _systemUnderTest.ApprovedPersonListPage(input) as ViewResult;

        ValidateOrganisationUsersModel(result, input.ExternalOrganisationId);

        result.ViewData.Should().ContainKey("BackLinkToDisplay");
        result.ViewData["BackLinkToDisplay"].Should().Be(PagePath.RegulatorCompanyDetail);

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
        await _systemUnderTest.ApprovedPersonListPage(externalId);

        // assert
        session.RegulatorSession.OrganisationId.Should().HaveValue();
        session.RegulatorSession.OrganisationId.Value.Should().Be(externalId);
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