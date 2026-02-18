namespace EPR.Common.Authorization.Extensions;

using Azure.Identity;
using Config;
using Constants;
using EPR.Common.Authorization.Services;
using EPR.Common.Authorization.Services.Interfaces;
using Handlers;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Requirements;
using Sessions;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void RegisterPolicy<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class, IHasUserData, new()
    {
        services.RegisterConfig(configuration);
        services.RegisterAuthorisation<T>(configuration);
        services.AddScoped<ISessionManager<T>, SessionManager<T>>();
    }

    public static IServiceCollection RegisterGraphServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterExtensionConfig(configuration);

        services.AddTransient<IGraphService, GraphService>();
        services.AddTransient(sp =>
        {
            var extensionOptions = sp.GetRequiredService<IOptions<AzureB2CExtensionConfig>>().Value;

            var credential = new ClientSecretCredential(
                extensionOptions.TenantId,
                extensionOptions.ClientId,
                extensionOptions.ClientSecret,
                new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                });

            return new GraphServiceClient(
                credential,
                ["https://graph.microsoft.com/.default"]);
        });

        return services;
    }

    public static IServiceCollection RegisterNullGraphServiceClient(this IServiceCollection services)
    {
        return services.AddTransient<IGraphService, NullGraphService>();
    }

    private static void RegisterConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EprAuthorizationConfig>(configuration.GetSection(EprAuthorizationConfig.SectionName));
    }

    private static void RegisterExtensionConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureB2CExtensionConfig>(configuration.GetSection(AzureB2CExtensionConfig.SectionName));
    }

    private static void RegisterAuthorisation<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class, IHasUserData, new()
    {
        services
            .AddHttpContextAccessor()
            .AddTransient<TokenHandler>()
            .AddAuthorizationCore(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();

                // set global authorisation for all actions on all controller
                // unless if attributed by [AllowAnonymous]
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy(
                    PolicyConstants.EprSelectSchemePolicy,
                    policy => policy.Requirements.Add(new EprSelectSchemePolicyRequirement()));

                options.AddPolicy(
                    PolicyConstants.EprFileUploadPolicy,
                    policy => policy.Requirements.Add(new EprFileUploadPolicyRequirement()));

                options.AddPolicy(
                    PolicyConstants.EprNonRegulatorRolesPolicy,
                    policy => policy.Requirements.Add(new EprNonRegulatorRolesPolicyRequirement()));

                options.AddPolicy(
                    PolicyConstants.AccountManagementPolicy,
                    policy => policy.Requirements.Add(new AccountManagementPolicyRequirement()));

                options.AddPolicy(
                    PolicyConstants.RegulatorBasicPolicy,
                    policy => policy.Requirements.Add(new RegulatorBasicPolicyRequirement()));

                options.AddPolicy(
                    PolicyConstants.RegulatorAdminPolicy,
                    policy => policy.Requirements.Add(new RegulatorAdminPolicyRequirement()));

                options.AddPolicy(
                   PolicyConstants.AccountPermissionManagementPolicy,
                   policy => policy.Requirements.Add(new AccountPermissionManagementPolicyRequirement()));
            })
            .AddScoped<IAuthorizationHandler, EprSelectSchemePolicyHandler<T>>()
            .AddScoped<IAuthorizationHandler, EprFileUploadPolicyHandler<T>>()
            .AddScoped<IAuthorizationHandler, EprNonRegulatorRolesPolicyHandler<T>>()
            .AddScoped<IAuthorizationHandler, AccountManagementPolicyHandler<T>>()
            .AddScoped<IAuthorizationHandler, AccountPermissionManagementPolicyHandler<T>>()
            .AddScoped<IAuthorizationHandler, RegulatorBasicPolicyHandler<T>>()
            .AddScoped<IAuthorizationHandler, RegulatorAdminPolicyHandler<T>>()
            .AddHttpClient(FacadeConstants.FacadeAPIClient, client =>
            {
                client.BaseAddress =
                    new Uri(configuration.GetValue<string>(
                        $"{EprAuthorizationConfig.SectionName}:{nameof(EprAuthorizationConfig.FacadeBaseUrl)}"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddHttpMessageHandler<TokenHandler>();
    }
}