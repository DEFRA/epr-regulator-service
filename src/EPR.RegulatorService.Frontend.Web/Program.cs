using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.FeatureManagement;
using EPR.RegulatorService.Frontend.Web.HealthChecks;
using EPR.RegulatorService.Frontend.Web.Middleware;
using EPR.RegulatorService.Frontend.Web.ModelBinders;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Logging;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, _, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName);
    ////config.Filter.ByExcluding(logEvent =>
    ////{
    ////    if (logEvent.Properties.TryGetValue("Headers.XArrLogId", out var value))
    ////    {
    ////        return !string.IsNullOrWhiteSpace(value.ToString());
    ////    }

    ////    return false;
    ////});
});

builder.Services
    .AddFeatureManagement().UseDisabledFeaturesHandler(new RedirectDisabledFeatureHandler());

builder.Services
    .RegisterCoreComponents(builder.Configuration)
    .RegisterWebComponents(builder.Configuration)
    .ConfigureMsalDistributedTokenOptions(builder.Configuration);

builder.Services
    .AddAntiforgery(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = builder.Configuration.GetValue<string>("CookieOptions:AntiForgeryCookieName");
    })
    .AddControllersWithViews(
        options => options.ModelBinderProviders.Insert(0, new PaymentMethodTypeModelBinderProvider()))
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddRazorPages();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.ForwardedHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:ForwardedHostHeaderName");
    options.OriginalHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:OriginalHostHeaderName");
    options.AllowedHosts = builder.Configuration.GetValue<string>("ForwardedHeaders:AllowedHosts").Split(";");
});

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddHealthChecks();

builder.Services.AddHsts(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" && context.Request.PathBase == PathString.Empty)
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});

app.UsePathBase(builder.Configuration.GetValue<string>("PATH_BASE"));

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseForwardedHeaders();

app.UseStaticFiles();
// after `UseStaticFiles()` to prevent logging of requests to css/js/png etc.
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (context, httpContext) =>
    {
        var request = httpContext.Request;

        context.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        context.Set("RequestId", httpContext.TraceIdentifier);
        context.Set("Method", request.Method);
        context.Set("Path", request.Path.Value);
        context.Set("UserAgent", request.Headers.UserAgent.ToString());
        context.Set("XArrLogId", request.Headers["X-ARR-LOG-ID"].ToString());
        context.Set("XAzureHealthProbe", request.Headers["X-Azure-Health-Probe"].ToString());
        context.Set("XForwardedFor", request.Headers["X-Forwarded-For"].ToString());
    };
});
app.UseMiddleware<SecurityHeaderMiddleware>();
app.UseCookiePolicy();
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks(
    builder.Configuration.GetValue<string>("HealthCheckPath"),
    HealthCheckOptionBuilder.Build()).AllowAnonymous();
app.UseMiddleware<UserDataCheckerMiddleware>();
app.UseRequestLocalization();
app.UseMiddleware<JourneyAccessCheckerMiddleware>();
app.UseMiddleware<AnalyticsCookieMiddleware>();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller}/{action}",
    defaults: new { controller = "Applications", action = "Applications" });

app.MapRazorPages();

await app.RunAsync();

namespace EPR.RegulatorService
{
    public partial class Program
    {
    }
}