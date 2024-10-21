namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Sessions;
    using EPR.RegulatorService.Frontend.Core.Models.Registrations;

    public abstract class RegistrationSubmissionsTestBase
    {
        private const string BackLinkViewDataKey = "BackLinkToDisplay";

        protected RegistrationSubmissionsController _controller = null!;
        protected Mock<HttpContext> _mockHttpContext = null!;
        protected Mock<IOptions<ExternalUrlsOptions>> _mockUrlsOptions = null!;
        protected Mock<IConfiguration> _mockConfiguration = null!;
        protected Mock<ISessionManager<JourneySession>>  _mockSessionManager { get; set; } = new Mock<ISessionManager<JourneySession>>();
        protected JourneySession _journeySession;
        private const string PowerBiLogin = "https://app.powerbi.com/";

        protected void SetupBase()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockUrlsOptions = new Mock<IOptions<ExternalUrlsOptions>>();
            _mockConfiguration = new Mock<IConfiguration>();

            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(section => section.Value).Returns("/regulators");
            _mockConfiguration.Setup(config => config.GetSection(ConfigKeys.PathBase))
                .Returns(mockConfigurationSection.Object);

            _mockUrlsOptions.Setup(mockUrlOptions =>
                mockUrlOptions.Value)
                .Returns(new ExternalUrlsOptions
                {
                    PowerBiLogin = PowerBiLogin
                });

            SetupJourneySession(null, null);

            _controller = new RegistrationSubmissionsController(
                _mockSessionManager.Object,
                _mockConfiguration.Object,
                _mockUrlsOptions.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                },
                TempData = new Mock<ITempDataDictionary>().Object
            };
        }

        public void SetupJourneySession(RegistrationFiltersModel registrationFiltersModel, Registration registration = null)
        {
            _journeySession = new JourneySession()
            {
                RegulatorRegistrationSession = new RegulatorRegistrationSession()
                {
                    RegistrationFiltersModel = registrationFiltersModel,
                    PageNumber = 1,
                    OrganisationRegistration = registration
                }
            };

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);
        }

        protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
        {
            bool hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
            hasBackLinkKey.Should().BeTrue();
            (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
        }
    }
}
