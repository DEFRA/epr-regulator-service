namespace EPR.RegulatorService.Frontend.Web.Extensions;

public static class ApplicationBuilderRedirectExtension
{
    /// <summary>
    /// Redirect one path to another bypassing PATH_BASE
    /// </summary>
    public static IApplicationBuilder UseAbsoluteRedirect(this IApplicationBuilder app, string path, string redirectTo) =>
        app.MapWhen(
            context => context.Request.Path.Value == path,
            branch => branch.Run(context =>
            {
                context.Response.Redirect(redirectTo);
                return Task.CompletedTask;
            }));
}
