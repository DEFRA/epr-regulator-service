namespace EPR.Common.Functions.Test.Extensions.DependencyInjection;

using EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;
using EPR.Common.Functions.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DependencyInjectionExtensions_AddMockAuthentication : BaseDependencyInjectionTests
{
    public DependencyInjectionExtensions_AddMockAuthentication()
    {
        this.CreateFunctionHostBuilder();
        var config = new ConfigurationManager();
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "TokenKey", "ThisIsASecretKeyForTestingPurposesOnly1234567890" },
            { "TokenTimeout", "60" },
        });
        this.Services.AddMockAuthentication(config);
    }

    [TestMethod]
    public void AddMockAuthentication_ShouldAddJwtAuthenticationManager()
    {
        this.AssertInterfaceRegistrationExists<IJwtAuthenticationManager>(ServiceLifetime.Singleton);
    }

    [TestMethod]
    public void AddMockAuthentication_ShouldAddJwtTokenRefresher()
    {
        this.AssertInterfaceRegistrationExists<IJwtTokenRefresher>(ServiceLifetime.Singleton);
    }

    [TestMethod]
    public void AddMockAuthentication_ShouldAddRefreshTokenGenerator()
    {
        this.AssertImplementationTypeRegistrationCorrect<IRefreshTokenGenerator, EPR.Common.Functions.AccessControl.MockAuthentication.RefreshTokenGenerator>(ServiceLifetime.Singleton);
    }

    [TestMethod]
    public void AddMockAuthentication_ShouldResolveServices()
    {
        using var scope = this.Services.BuildServiceProvider().CreateScope();

        var authManager = scope.ServiceProvider.GetService<IJwtAuthenticationManager>();
        var tokenRefresher = scope.ServiceProvider.GetService<IJwtTokenRefresher>();
        var refreshGenerator = scope.ServiceProvider.GetService<IRefreshTokenGenerator>();

        authManager.Should().NotBeNull();
        tokenRefresher.Should().NotBeNull();
        refreshGenerator.Should().NotBeNull();
    }
}
