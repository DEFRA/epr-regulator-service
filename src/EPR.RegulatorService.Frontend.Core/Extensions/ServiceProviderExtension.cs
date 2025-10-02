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
        bool useMockDataForFacade = configuration.GetValue<bool>("FacadeApi:UseMockData");

        if (useMockDataForFacade)
        {
            services.AddSingleton<IFacadeService, MockedFacadeService>();
        }
        else
        {
            services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")))
                 .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                 {
                     PooledConnectionLifetime = TimeSpan.FromMinutes(5), // optional, configurable
                     SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                     {
                         EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                             | System.Security.Authentication.SslProtocols.Tls13
                     }
                 });
        }

        bool useMockDataForRegistrationFacade = configuration.GetValue<bool>("ReprocessorExporterFacadeApi:UseMockData");

        if (useMockDataForRegistrationFacade)
        {
            services.AddSingleton<IReprocessorExporterService, MockedReprocessorExporterService>();
        }
        else
        {
            services.AddHttpClient<IReprocessorExporterService, ReprocessorExporterService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")))
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(5), // optional, configurable
                        SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                        {
                            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                                | System.Security.Authentication.SslProtocols.Tls13
                        }
                    });
        }

        services.AddHttpClient<IPaymentFacadeService, PaymentFacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("PaymentFacadeApi:TimeoutSeconds")))
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5), // optional, configurable
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                            | System.Security.Authentication.SslProtocols.Tls13
                    }
                });

        services.AddScoped<ISubmissionFilterConfigService, SubmissionFilterConfigService>();

        return services;
    }
}