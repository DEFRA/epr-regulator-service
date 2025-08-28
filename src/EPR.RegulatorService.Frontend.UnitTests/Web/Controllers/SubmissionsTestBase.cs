using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System.Security.Claims;
namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

using EPR.RegulatorService.Frontend.Core.Configs;

using Microsoft.AspNetCore.Mvc.ViewFeatures;

public abstract class SubmissionsTestBase
{
    protected const string ModelErrorKey = "Error";
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    protected SubmissionsController _systemUnderTest = null!;
    protected Mock<HttpContext> _httpContextMock = null!;
    protected Mock<ClaimsPrincipal> _userMock = null!;
    protected Mock<ISessionManager<JourneySession>> _sessionManagerMock = null!;
    protected Mock<IFacadeService> _facadeServiceMock = null!;
    protected Mock<ISubmissionFilterConfigService> _submissionFilterConfigServiceMock = null!;
    protected Mock<IPaymentFacadeService> _paymentFacadeServiceMock = null!;
    protected JourneySession JourneySessionMock { get; set; }
    protected Mock<IOptions<SubmissionFiltersConfig>> _submissionFiltersMock = null!;
    protected Mock<IOptions<ExternalUrlsOptions>> _urlsOptionMock = null!;
    protected Mock<IConfiguration> _configurationMock = null!;
    protected ITempDataDictionary _tempDataDictionary = null!;
    private const string PowerBiLogin = "https://app.powerbi.com/";

    protected void SetupBase(UserData userData = null)
    {
        _httpContextMock = new Mock<HttpContext>();
        _userMock = new Mock<ClaimsPrincipal>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _submissionFiltersMock = new Mock<IOptions<SubmissionFiltersConfig>>();
        _urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        _tempDataDictionary = new TempDataDictionary(_httpContextMock.Object, Mock.Of<ITempDataProvider>());
        _facadeServiceMock = new Mock<IFacadeService>();
        _submissionFilterConfigServiceMock = new Mock<ISubmissionFilterConfigService>();
        _paymentFacadeServiceMock = new Mock<IPaymentFacadeService>();
        _configurationMock = new Mock<IConfiguration>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        configurationSectionMock.Setup(section => section.Value).Returns("/regulators");
        _configurationMock.Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        SetUpUserData(userData);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult(new JourneySession()));

        _submissionFiltersMock.Setup(mock => mock.Value).Returns(new SubmissionFiltersConfig
        {
            Years = [2023, 2024],
            PomPeriods = ["January to June 2023", "July to December 2023", "January to June 2024", "July to December 2024"
            ]
        });

        _urlsOptionMock.Setup(mock => mock.Value).Returns(new ExternalUrlsOptions
        {
            PowerBiLogin = PowerBiLogin
        });

        _systemUnderTest = new SubmissionsController(
            _sessionManagerMock.Object,
            _configurationMock.Object,
            _submissionFiltersMock.Object,
            _urlsOptionMock.Object,
            _facadeServiceMock.Object,
            _submissionFilterConfigServiceMock.Object,
            _paymentFacadeServiceMock.Object);

        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
        _systemUnderTest.TempData = _tempDataDictionary;
    }

    private void SetUpUserData(UserData? userData)
    {
        var claims = new List<Claim>();
        if (userData != null)
        {
            claims.Add(new(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userData)));
        }

        _userMock.Setup(x => x.Claims).Returns(claims);
        _httpContextMock.Setup(x => x.User).Returns(_userMock.Object);
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}