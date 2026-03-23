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
        services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));

        services.AddHttpClient<IReprocessorExporterService, ReprocessorExporterService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));

        services.AddHttpClient<IPaymentFacadeService, PaymentFacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("PaymentFacadeApi:TimeoutSeconds")));

        services.AddScoped<ISubmissionFilterConfigService, SubmissionFilterConfigService>();

        return services;
    }
}