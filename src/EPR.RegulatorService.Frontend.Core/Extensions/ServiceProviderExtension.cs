using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EPR.RegulatorService.Frontend.Core.Services;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceProviderExtension
{
    public static IServiceCollection RegisterCoreComponents(this IServiceCollection services, IConfiguration configuration)
    {
        bool useMockData = configuration.GetValue<bool>("FacadeApi:UseMockData");
        if (useMockData)
        {
            services.AddSingleton<IFacadeService, MockedFacadeService>();
        }
        else
        {
            services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));
        }

        return services;
    }
}
