using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

public abstract class RegistrationControllerTestBase
{
    protected Mock<ISessionManager<JourneySession>> _sessionManagerMock = null!;
    protected Mock<IMapper> _mapperMock = null!;
    protected Mock<IReprocessorExporterService> _reprocessorExporterServiceMock = null!;
    protected Mock<HttpContext> _httpContextMock = null!;

    protected JourneySession _journeySession = null!;

    protected void CreateCommonMocks()
    {
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _mapperMock = new Mock<IMapper>();
        _reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();
        _httpContextMock = new Mock<HttpContext>();
    }

    public static Mock<IConfiguration> CreateConfigurationMock()
    {
        var configurationSectionMock = new Mock<IConfigurationSection>();
        
        configurationSectionMock
            .Setup(section => section.Value)
            .Returns("/regulators");

        var configurationMock = new Mock<IConfiguration>();

        configurationMock
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        return configurationMock;
    }

    protected void CreateSessionMock()
    {
        _journeySession = new JourneySession
        {
            ReprocessorExporterSession = new ReprocessorExporterSession()
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_journeySession);
    }
}