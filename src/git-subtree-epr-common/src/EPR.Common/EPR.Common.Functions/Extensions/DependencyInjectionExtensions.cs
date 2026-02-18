namespace EPR.Common.Functions.Extensions;

using System.Text;
using AccessControl;
using AccessControl.Interfaces;
using AccessControl.MockAuthentication;
using AccessControl.MockAuthentication.Interfaces;
using CancellationTokens;
using CancellationTokens.Interfaces;
using Database.Decorators;
using Database.Decorators.Interfaces;
using Database.Repositories;
using Database.Repositories.Interfaces;
using Database.UnitOfWork;
using Database.UnitOfWork.Interfaces;
using Http;
using Http.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Services;
using Services.Interfaces;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCommonDatabaseServices(this IServiceCollection services) =>
        services.AddScoped<IDatabaseQueryRepository, DatabaseQueryRepository>()
            .AddScoped<IRequestTimeService, RequestTimeService>()
            .AddTransient<IUnitOfWork, UnitOfWork>()
            .AddTransient<IEntityDecorator, CreatedUpdatedDecorator>();

    public static IServiceCollection AddCommonServices(this IServiceCollection services) =>
        services
            .AddSingleton<ILoggerFactory, LoggerFactory>()
            .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
            .AddTransient<ITimeService, TimeService>()
            .AddTransient<IHttpContextAccessor, HttpContextAccessor>()
            .AddScoped<ICancellationTokenAccessor, CancellationTokenAccessor>();

    public static IServiceCollection AddMockAuthentication(this IServiceCollection services, ConfigurationManager config)
    {
        var tokenKey = config.GetValue<string>("TokenKey");
        var tokenTimeout = config.GetValue<int>("TokenTimeout");
        services
            .AddSingleton<IJwtTokenRefresher>(x => new JwtTokenRefresher(Encoding.ASCII.GetBytes(tokenKey), x.GetService<IJwtAuthenticationManager>()))
            .AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>()
            .AddSingleton<IJwtAuthenticationManager>(x => new JwtAuthenticationManager(tokenKey, tokenTimeout, x.GetService<IRefreshTokenGenerator>()))
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

        return services;
    }

    public static IServiceCollection AddEprAccessControl(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticator, HttpAuthenticator>();

        return services.AddEprAccessControl<CommonPermission>()
            .AddContextProviders();
    }

    private static IServiceCollection AddContextProviders(this IServiceCollection services) =>
        services
            .AddScoped<ContextAdminOverride>()
            .AddScoped<IUserContextProvider, UserContextProvider>();

    private static IServiceCollection AddEprAccessControl<TPermission>(this IServiceCollection services)
    {
        return services.AddScoped<IHttpRequestWrapper<TPermission>, HttpRequestWrapper<TPermission>>();
    }
}