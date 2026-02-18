namespace EPR.Common.Functions.Test.Extensions.DependencyInjection
{
    using EPR.Common.Functions.Database.Context.Interfaces;
    using EPR.Common.Functions.Database.Decorators;
    using EPR.Common.Functions.Database.Decorators.Interfaces;
    using EPR.Common.Functions.Database.Repositories;
    using EPR.Common.Functions.Database.Repositories.Interfaces;
    using EPR.Common.Functions.Database.UnitOfWork;
    using EPR.Common.Functions.Database.UnitOfWork.Interfaces;
    using EPR.Common.Functions.Extensions;
    using EPR.Common.Functions.Services;
    using EPR.Common.Functions.Services.Interfaces;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestContext = EPR.Common.Functions.Test.Database.TestContexts.TestContext;

    [TestClass]
    public class DependencyInjectionExtensions_AddCommonDatabaseServices : BaseDependencyInjectionTests
    {
        public DependencyInjectionExtensions_AddCommonDatabaseServices()
        {
            this.CreateFunctionHostBuilder();
            this.Services
                .AddCommonServices()
                .AddCommonDatabaseServices()
                .AddEprAccessControl();

            this.Services.AddLogging()
                .AddDbContext<IEprCommonContext, TestContext>()
                .AddScoped<ITimeService, TimeService>();
        }

        [TestMethod]
        public void AddCommonDatabaseServices_ShouldAddDatabaseQueryRepository()
        {
            this.AssertImplementationTypeRegistrationCorrect<IDatabaseQueryRepository, DatabaseQueryRepository>(ServiceLifetime.Scoped);
        }

        [TestMethod]
        public void AddCommonDatabaseServices_ShouldAddRequestTimeService()
        {
            this.AssertImplementationTypeRegistrationCorrect<IRequestTimeService, RequestTimeService>(ServiceLifetime.Scoped);
        }

        [TestMethod]
        public void AddCommonDatabaseServices_ShouldAddUnitOfWork()
        {
            this.AssertImplementationTypeRegistrationCorrect<IUnitOfWork, UnitOfWork>(ServiceLifetime.Transient);
        }

        [TestMethod]
        public void AddCommonDatabaseServices_ShouldAddDecorators()
        {
            this.Services.Where(x => x.ServiceType == typeof(IEntityDecorator)).Should().AllSatisfy(x => x.Lifetime.Should().Be(ServiceLifetime.Transient));

            using (var scope = this.Services.BuildServiceProvider().CreateScope())
            {
                var decorators = scope.ServiceProvider.GetRequiredService<IEnumerable<IEntityDecorator>>();
                decorators.Should().HaveCount(1);
                decorators.Should().ContainItemsAssignableTo<CreatedUpdatedDecorator>();
            }
        }
    }
}