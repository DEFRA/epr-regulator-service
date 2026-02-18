namespace EPR.Common.Logging.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Clients;
using Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Services;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        return services.RegisterConfiguration()
            .RegisterServices()
            .RegisterHttpClient();
    }

    private static IServiceCollection RegisterConfiguration(this IServiceCollection services) =>
        services.ConfigureSection<LoggingApiConfig>(LoggingApiConfig.SectionName);

    private static IServiceCollection RegisterServices(this IServiceCollection services) =>
        services.AddScoped<ILoggingService, LoggingService>();

    private static IServiceCollection RegisterHttpClient(this IServiceCollection services)
    {
        services
            .AddHttpClient<ILoggingApiClient, LoggingApiClient>((sp, c) =>
            {
                var loggingApiConfig = sp.GetRequiredService<IOptions<LoggingApiConfig>>().Value;
                c.BaseAddress = new Uri($"{loggingApiConfig.BaseUrl}/api/v1/");
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });
        return services;
    }
}
