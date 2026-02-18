namespace EPR.Common.Functions.Test.Extensions.DependencyInjection;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

public abstract class BaseDependencyInjectionTests
    {
        protected IServiceCollection Services { get; set; }

        protected void CreateFunctionHostBuilder()
        {
            this.Services = new ServiceCollection();
            this.Services.AddSingleton(Substitute.For<IConfiguration>());
        }

        protected void AssertImplementationTypeRegistrationCorrect<TInterface, TImplementation>(ServiceLifetime serviceLifetime)
        {
            var serviceDescription = this.Services.Single(x => x.ServiceType == typeof(TInterface));
            serviceDescription.ImplementationType.Should().Be(typeof(TImplementation));
            serviceDescription.Lifetime.Should().Be(serviceLifetime);
            this.AssertRegistrationCorrect<TInterface, TImplementation>(serviceDescription, serviceLifetime);
        }

        protected void AssertImplementationInstanceRegistrationCorrect<TInterface, TImplementation>(ServiceLifetime serviceLifetime)
        {
            var serviceDescription = this.Services.Single(x => x.ServiceType == typeof(TInterface));
            serviceDescription.ImplementationInstance.Should().BeOfType(typeof(TImplementation));
            serviceDescription.Lifetime.Should().Be(serviceLifetime);
            this.AssertRegistrationCorrect<TInterface, TImplementation>(serviceDescription, serviceLifetime);
        }

        protected void AssertRegistrationExists<TInterface, TImplementation>(ServiceLifetime serviceLifetime)
        {
            var serviceDescription = this.Services.Single(x => x.ServiceType == typeof(TInterface));
            serviceDescription.ImplementationType.Should().Be(typeof(TImplementation));
            serviceDescription.Lifetime.Should().Be(serviceLifetime);
        }

        protected void AssertInterfaceRegistrationExists<TInterface>(ServiceLifetime serviceLifetime)
        {
            var serviceDescription = this.Services.Single(x => x.ServiceType == typeof(TInterface));
            serviceDescription.Lifetime.Should().Be(serviceLifetime);
        }

        private void AssertRegistrationCorrect<TInterface, TImplementation>(ServiceDescriptor serviceDescription, ServiceLifetime serviceLifetime)
        {
            serviceDescription.Lifetime.Should().Be(serviceLifetime);
            using (var scope = this.Services.BuildServiceProvider().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<TInterface>().Should().BeOfType(typeof(TImplementation));
            }
        }
    }