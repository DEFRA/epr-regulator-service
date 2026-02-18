namespace EPR.Common.Functions.Test.Extensions.DependencyInjection
{
    using EPR.Common.Functions.CancellationTokens;
    using EPR.Common.Functions.CancellationTokens.Interfaces;
    using EPR.Common.Functions.Extensions;
    using EPR.Common.Functions.Services;
    using EPR.Common.Functions.Services.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DependencyInjectionExtensions_AddCommonServices : BaseDependencyInjectionTests
    {
        public DependencyInjectionExtensions_AddCommonServices()
        {
            this.CreateFunctionHostBuilder();
            this.Services
                .AddCommonServices();
        }

        [TestMethod]
        public void AddCommonServices_ShouldAddTimeService()
        {
            this.AssertImplementationTypeRegistrationCorrect<ITimeService, TimeService>(ServiceLifetime.Transient);
        }

        [TestMethod]
        public void AddCommonServices_ShouldAddHttpContextAccessor()
        {
            this.AssertImplementationTypeRegistrationCorrect<IHttpContextAccessor, HttpContextAccessor>(ServiceLifetime.Transient);
        }

        [TestMethod]
        public void AddCommonServices_ShouldAddCancellationTokenAccessor()
        {
            this.AssertImplementationTypeRegistrationCorrect<ICancellationTokenAccessor, CancellationTokenAccessor>(ServiceLifetime.Scoped);
        }
    }
}