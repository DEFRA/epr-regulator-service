using System.Diagnostics.CodeAnalysis;

using EPR.Common.Authorization.Extensions;
using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Cookies;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.Middleware;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.Validations;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

using StackExchange.Redis;

namespace EPR.RegulatorService.Frontend.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceProviderExtension
{
    public static IServiceCollection RegisterWebComponents(this IServiceCollection services, IConfiguration configuration)
    {
        SetTempDataCookieOptions(services, configuration);
        ConfigureOptions(services, configuration);
        ConfigureLocalization(services);
        ConfigureAuthentication(services, configuration);
        ConfigureAuthorization(services, configuration);
        ConfigureSession(services, configuration);
        RegisterServices(services);
        RegisterFluentValidation(services);
        RegisterAutoMapper(services);

        return services;
    }

    public static IServiceCollection ConfigureMsalDistributedTokenOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights());
        var buildLogger = loggerFactory.CreateLogger<Program>();

        services.Configure<MsalDistributedTokenCacheAdapterOptions>(options =>
        {
            options.DisableL1Cache = configuration.GetValue("MsalOptions:DisableL1Cache", true);
            options.SlidingExpiration = TimeSpan.FromMinutes(configuration.GetValue("MsalOptions:L2SlidingExpiration", 20));

            options.OnL2CacheFailure = exception =>
            {
                if (exception is RedisConnectionException)
                {
                    buildLogger.LogError(exception, "L2 Cache Failure Redis connection exception: {Message}", exception.Message);
                    return true;
                }

                buildLogger.LogError(exception, "L2 Cache Failure: {Message}", exception.Message);
                return false;
            };
        });
        return services;
    }

    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EprCookieOptions>(configuration.GetSection(EprCookieOptions.ConfigSection));
        services.Configure<AnalyticsOptions>(configuration.GetSection(AnalyticsOptions.ConfigSection));
        services.Configure<PhaseBannerOptions>(configuration.GetSection(PhaseBannerOptions.ConfigSection));
        services.Configure<SubmissionFiltersConfig>(configuration.GetSection(SubmissionFiltersConfig.ConfigSection));
        services.Configure<ExternalUrlsOptions>(configuration.GetSection(ExternalUrlsOptions.ConfigSection));
        services.Configure<EmailAddressOptions>(configuration.GetSection(EmailAddressOptions.ConfigSection));
        services.Configure<SiteDateOptions>(configuration.GetSection(SiteDateOptions.ConfigSection));
        services.Configure<ServiceSettingsOptions>(configuration.GetSection(ServiceSettingsOptions.ConfigSection));
        services.Configure<FacadeApiConfig>(configuration.GetSection(FacadeApiConfig.ConfigSection));
        services.Configure<PaginationConfig>(configuration.GetSection(PaginationConfig.ConfigSection));
        services.Configure<TransferOrganisationConfig>(configuration.GetSection(TransferOrganisationConfig.ConfigSection));
        services.Configure<LandingPageConfig>(configuration.GetSection(LandingPageConfig.ConfigSection));
        services.Configure<PaymentFacadeApiConfig>(configuration.GetSection(PaymentFacadeApiConfig.ConfigSection));
        services.Configure<PaymentDetailsOptions>(configuration.GetSection(PaymentDetailsOptions.ConfigSection));
        services.Configure<ReprocessorExporterFacadeApiConfig>(configuration.GetSection(ReprocessorExporterFacadeApiConfig.ConfigSection));
        services.Configure<PackagingDataSubmissionsConfig>(configuration.GetSection(PackagingDataSubmissionsConfig.ConfigSection));
        services.Configure<RegistrationSubmissionsOptions>(configuration.GetSection(RegistrationSubmissionsOptions.ConfigSection));
        services.Configure<ReprocessorExporterConfig>(configuration.GetSection(ReprocessorExporterConfig.ConfigSection));
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICookieService, CookieService>();
        services.AddScoped<ISessionManager<JourneySession>, JourneySessionManager>();
        services.AddTransient<UserDataCheckerMiddleware>();
    }

    private static void SetTempDataCookieOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CookieTempDataProviderOptions>(options =>
        {
            options.Cookie.Name = configuration.GetValue<string>("CookieOptions:TempDataCookie");
            options.Cookie.Path = configuration.GetValue<string>("PATH_BASE");
        });
    }

    private static void ConfigureLocalization(IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources")
            .Configure<RequestLocalizationOptions>(options =>
            {
                var cultureList = new[] { Language.English, Language.Welsh };
                options.SetDefaultCulture(Language.English);
                options.AddSupportedCultures(cultureList);
                options.AddSupportedUICultures(cultureList);
                options.RequestCultureProviders = new IRequestCultureProvider[]
                {
                    new SessionRequestCultureProvider()
                };
            });
    }

    private static void ConfigureSession(IServiceCollection services, IConfiguration configuration)
    {
        var useLocalSession = configuration.GetValue<bool>("UseLocalSession");

        if (!useLocalSession)
        {
            var redisConnection = configuration.GetConnectionString("REDIS_CONNECTION");

            services.AddDataProtection()
                .SetApplicationName("EprProducers")
                .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnection), "DataProtection-Keys");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = configuration.GetValue<string>("RedisInstanceName");
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSession(options =>
        {
            options.Cookie.Name = configuration.GetValue<string>("CookieOptions:SessionCookieName");
            options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionIdleTimeOutMinutes"));
            options.Cookie.IsEssential = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Path = "/";
        });

    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                configuration.GetSection("AzureAdB2C").Bind(options);
                options.ErrorPath = "/error";
            }, options =>
            {
                options.Cookie.Name = configuration.GetValue<string>("CookieOptions:AuthenticationCookieName");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue<int>("CookieOptions:AuthenticationExpiryInMinutes"));
                options.SlidingExpiration = true;
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .EnableTokenAcquisitionToCallDownstreamApi(new string[] { configuration.GetValue<string>("FacadeAPI:DownstreamScope") })
            .AddDistributedTokenCaches();
    }

    private static void ConfigureAuthorization(IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterPolicy<JourneySession>(configuration);
    }

    private static void RegisterFluentValidation(IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<IdRequestValidator>()
                .AddValidatorsFromAssemblyContaining<PaymentDateViewModelValidator>();
    }

    private static void RegisterAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ManageRegistrationsMappingProfile));
    }
}
