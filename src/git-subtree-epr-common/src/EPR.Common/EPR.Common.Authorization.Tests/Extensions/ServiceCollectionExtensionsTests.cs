using EPR.Common.Authorization.Config;
using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Services;
using EPR.Common.Authorization.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace EPR.Common.Authorization.Test.Extensions;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void RegisterGraphServiceClient_RegistersExpectedServices()
    {
        // Arrange
        var buildConfiguration = () => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { $"{AzureB2CExtensionConfig.SectionName}:{nameof(AzureB2CExtensionConfig.ClientId)}", "test-client-id" },
                { $"{AzureB2CExtensionConfig.SectionName}:{nameof(AzureB2CExtensionConfig.ClientSecret)}", "test-secret" },
                { $"{AzureB2CExtensionConfig.SectionName}:{nameof(AzureB2CExtensionConfig.TenantId)}", "test-tenant" },
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = buildConfiguration();

        // Act
        services.RegisterGraphServiceClient(configuration);
        var provider = services.BuildServiceProvider();

        var graphService = provider.GetRequiredService<IGraphService>();
        graphService.Should().BeOfType<GraphService>();

        var options = provider.GetRequiredService<IOptions<AzureB2CExtensionConfig>>().Value;
        options.ClientId.Should().Be("test-client-id");
        options.ClientSecret.Should().Be("test-secret");
        options.TenantId.Should().Be("test-tenant");
    }

    [TestMethod]
    public void RegisterNullGraphServiceClient_RegistersConcreteNullGraphService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.RegisterNullGraphServiceClient();

        // Assert
        services.Any(d => d.ImplementationType == typeof(EPR.Common.Authorization.Services.NullGraphService))
            .Should().BeTrue("only a factory returning null is registered");
    }
}