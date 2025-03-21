using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.RegulatorService.Frontend.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceProviderExtension
{
    public static IServiceCollection RegisterCoreComponents(this IServiceCollection services, IConfiguration configuration)
    {
        var useMockData = configuration.GetValue<bool>("FacadeApi:UseMockData");
        if (useMockData)
        {
            services.AddSingleton<IFacadeService, MockedFacadeService>();
            services.AddSingleton<IRegistrationService, MockedRegistrationService>();
        }
        else
        {
            services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));

            // Register the real RegistrationService here when implemented
            //services.AddScoped<IRegistrationService, RegistrationService>();
        }

        services.AddHttpClient<IPaymentFacadeService, PaymentFacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("PaymentFacadeApi:TimeoutSeconds")));


        return services;
    }
}
