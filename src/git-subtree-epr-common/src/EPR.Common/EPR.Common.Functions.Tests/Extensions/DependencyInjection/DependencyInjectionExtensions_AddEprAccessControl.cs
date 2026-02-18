namespace EPR.Common.Functions.Test.Extensions.DependencyInjection;

using EPR.Common.Functions.AccessControl;
using EPR.Common.Functions.AccessControl.Interfaces;
using EPR.Common.Functions.Extensions;
using EPR.Common.Functions.Http.Interfaces;
using EPR.Common.Functions.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DependencyInjectionExtensions_AddEprAccessControl : BaseDependencyInjectionTests
{
    public DependencyInjectionExtensions_AddEprAccessControl()
    {
        this.CreateFunctionHostBuilder();
        this.Services
            .AddLogging()
            .AddCommonServices()
            .AddEprAccessControl();
    }

    [TestMethod]
    public void AddEprAccessControl_ShouldAddAuthenticator()
    {
        this.AssertImplementationTypeRegistrationCorrect<IAuthenticator, HttpAuthenticator>(ServiceLifetime.Scoped);
    }

    [TestMethod]
    public void AddEprAccessControl_ShouldAddUserContextProvider()
    {
        this.AssertImplementationTypeRegistrationCorrect<IUserContextProvider, UserContextProvider>(ServiceLifetime.Scoped);
    }

    [TestMethod]
    public void AddEprAccessControl_ShouldAddHttpRequestWrapper()
    {
        var serviceDescription = this.Services.SingleOrDefault(x => x.ServiceType == typeof(IHttpRequestWrapper<CommonPermission>));

        serviceDescription.Should().NotBeNull();
        serviceDescription.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [TestMethod]
    public void AddEprAccessControl_ShouldAddContextAdminOverride()
    {
        var serviceDescription = this.Services.SingleOrDefault(x => x.ServiceType == typeof(ContextAdminOverride));

        serviceDescription.Should().NotBeNull();
        serviceDescription.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}
