using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Applications;
using EPR.RegulatorService.Frontend.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    public abstract class ApplicationsTestBase
    {
        protected const string ModelErrorKey = "Error";
        private const string BackLinkViewDataKey = "BackLinkToDisplay";
        protected Mock<HttpContext> _httpContextMock = null!;
        protected Mock<ClaimsPrincipal> _userMock = null!;
        protected Mock<ISessionManager<JourneySession>> _sessionManagerMock = null!;
        protected Mock<IFacadeService> _facadeServiceMock = null!;
        protected Mock<IOptions<ExternalUrlsOptions>> _urlsOptionMock = null!;
        protected Mock<ILogger<ApplicationsController>> _loggerMock = null!;
        protected Mock<ITempDataDictionary> _tempDataDictionaryMock = null!;
        protected ApplicationsController _systemUnderTest = null!;
        protected Mock<IOptions<TransferOrganisationConfig>> _transferOrganisationConfig = null!;
        protected Mock<IConfiguration> _configurationMock = null!;
        protected JourneySession? JourneySessionMock { get; set; }

        protected void SetupBase(UserData? userData = null)
        {
            _httpContextMock = new Mock<HttpContext>();
            _userMock = new Mock<ClaimsPrincipal>();
            _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
            _facadeServiceMock = new Mock<IFacadeService>();
            _urlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
            _tempDataDictionaryMock = new Mock<ITempDataDictionary>();
            _transferOrganisationConfig = new Mock<IOptions<TransferOrganisationConfig>>();
            _transferOrganisationConfig.Setup(x => x.Value).Returns(GetRegulatorOrganisations());
            
            _configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(section => section.Value).Returns("/regulators");
            _configurationMock.Setup(config => config.GetSection(ConfigKeys.PathBase))
                .Returns(configurationSectionMock.Object);
            
            SetUpUserData(userData);

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult(JourneySessionMock));
           
            _loggerMock = new Mock<ILogger<ApplicationsController>>();
            _tempDataDictionaryMock = new Mock<ITempDataDictionary>();

            _systemUnderTest = new ApplicationsController(_sessionManagerMock.Object, _facadeServiceMock.Object,
                _transferOrganisationConfig.Object, _configurationMock.Object, _loggerMock.Object);

            _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
            _systemUnderTest.TempData = _tempDataDictionaryMock.Object;
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

        public static TransferOrganisationConfig GetRegulatorOrganisations() =>
            new()
            {
                Organisations = "{\"data\":[{\"KeyName\":\"England\",\"KeyValue\":\"Environment Agency\",\"OrganisationTypeId\":6,\"NationId\":1},{\"KeyName\":\"Northern Ireland\",\"KeyValue\":\"Northern Ireland Environment Agency\",\"OrganisationTypeId\":6,\"NationId\":2},{\"KeyName\":\"Scotland\",\"KeyValue\":\"Scottish Environment Protection Agency\",\"OrganisationTypeId\":6,\"NationId\":3},{\"KeyName\":\"Wales\",\"KeyValue\":\"Natural Resources Wales\",\"OrganisationTypeId\":6,\"NationId\":4}]}"
            };

        protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
        {
            bool hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out object? gotBackLinkObject);
            hasBackLinkKey.Should().BeTrue();
            (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
        }

        protected void SetupRegulatorSession(Guid organisationId, RejectUserJourneyData? rejectUserJourneyData = null) =>
            _sessionManagerMock.Setup(
                    x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(new JourneySession
                {
                    RegulatorSession = new RegulatorSession
                    {
                        OrganisationId = organisationId,
                        RejectUserJourneyData = rejectUserJourneyData
                    }
                });
    }
}

