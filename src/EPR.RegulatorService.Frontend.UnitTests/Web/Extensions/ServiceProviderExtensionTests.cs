using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using Moq;
using System.Text.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Extensions;

[TestClass]
public class ServiceProviderExtensionTests
{
    private IConfiguration _configuration;
    private readonly string _organisations = "{\"data\":[]}";

    [TestMethod]
    public void ConfigureMsalDistributedTokenOptions_RegistersMsalDistributedTokenCacheAdapterOptions()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceCollection>();
        var mockConfig = new Mock<IConfiguration>();
        var descriptors = new List<ServiceDescriptor>();
        mockServiceProvider.Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor a) => descriptors.Add(a));

        // Act
        mockServiceProvider.Object.ConfigureMsalDistributedTokenOptions(mockConfig.Object);

        // Assert
        mockServiceProvider.Verify(m => m.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeastOnce);
        Assert.IsTrue(descriptors.Any(d => d.ServiceType == typeof(IConfigureOptions<MsalDistributedTokenCacheAdapterOptions>)));
    }

    [TestMethod]
    public void GivenRegisterWebComponents_WithLocalSession_ThenShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            {EprCookieOptions.ConfigSection, JsonSerializer.Serialize(new EprCookieOptions{TempDataCookie = "testing_cookie" })},
            {AnalyticsOptions.ConfigSection, JsonSerializer.Serialize(new AnalyticsOptions())},
            {PhaseBannerOptions.ConfigSection, JsonSerializer.Serialize(new PhaseBannerOptions())},
            {ExternalUrlsOptions.ConfigSection, JsonSerializer.Serialize(new ExternalUrlsOptions())},
            {EmailAddressOptions.ConfigSection, JsonSerializer.Serialize(new EmailAddressOptions())},
            {SiteDateOptions.ConfigSection, JsonSerializer.Serialize(new SiteDateOptions())},
            {ServiceSettingsOptions.ConfigSection, JsonSerializer.Serialize(new ServiceSettingsOptions())},
            {FacadeApiConfig.ConfigSection, JsonSerializer.Serialize(new FacadeApiConfig())},
            {PaginationConfig.ConfigSection, JsonSerializer.Serialize(new PaginationConfig())},
            {TransferOrganisationConfig.ConfigSection, JsonSerializer.Serialize(new TransferOrganisationConfig(){Organisations = _organisations }) },
            {LandingPageConfig.ConfigSection, JsonSerializer.Serialize(new LandingPageConfig()) },
            {"UseLocalSession", "true"},
            {"PATH_BASE", "~/home" }
        })
        .Build();
               
        // Act
        services.RegisterWebComponents(_configuration);

        // Assert
        Assert.IsNotNull(services);
        services.Count.Should().BeGreaterThan(0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GivenRegisterWebComponents_WithNotSession_ThenShouldThrowError()
    {
        // Test method is failing due to being unable to retrive need to work out how to enable this as the code below does not work
        //_configuration.GetSection("ConnectionStrings")["REDIS_CONNECTION"];

        // Arrange
        var services = new ServiceCollection();
        var connectionStrings = new Dictionary<string, string> { { "REDIS_CONNECTION", "localhost:6379" } };

        _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
                    {EprCookieOptions.ConfigSection, JsonSerializer.Serialize(new EprCookieOptions())},
                    {AnalyticsOptions.ConfigSection, JsonSerializer.Serialize(new AnalyticsOptions())},
                    {PhaseBannerOptions.ConfigSection, JsonSerializer.Serialize(new PhaseBannerOptions())},
                    {ExternalUrlsOptions.ConfigSection, JsonSerializer.Serialize(new ExternalUrlsOptions())},
                    {EmailAddressOptions.ConfigSection, JsonSerializer.Serialize(new EmailAddressOptions())},
                    {SiteDateOptions.ConfigSection, JsonSerializer.Serialize(new SiteDateOptions())},
                    {ServiceSettingsOptions.ConfigSection, JsonSerializer.Serialize(new ServiceSettingsOptions())},
                    {FacadeApiConfig.ConfigSection, JsonSerializer.Serialize(new FacadeApiConfig())},
                    {PaginationConfig.ConfigSection, JsonSerializer.Serialize(new PaginationConfig())},
                    {TransferOrganisationConfig.ConfigSection, JsonSerializer.Serialize(new TransferOrganisationConfig(){Organisations = _organisations }) },
                    {LandingPageConfig.ConfigSection, JsonSerializer.Serialize(new LandingPageConfig()) },
                    {"UseLocalSession", "false"},
                    {"ConnectionStrings", JsonSerializer.Serialize(connectionStrings) }
        })
        .Build();

        // Act
        services.RegisterWebComponents(_configuration);

        // Assert
        Assert.IsNull(services);    
    }
}