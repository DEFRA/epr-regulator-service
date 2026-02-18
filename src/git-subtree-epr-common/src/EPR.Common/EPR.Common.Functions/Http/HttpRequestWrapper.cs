namespace EPR.Common.Functions.Http
{
    using System.Net;
    using System.Runtime.CompilerServices;
    using AccessControl.Interfaces;
    using CancellationTokens.Interfaces;
    using Exceptions;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public sealed class HttpRequestWrapper<TPermission> : IHttpRequestWrapper<TPermission>
    {
        private readonly ILogger<HttpRequestWrapper<TPermission>> logger;
        private readonly IAuthenticator authenticator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserContextProvider userContextProvider;
        private readonly ICancellationTokenAccessor cancellationTokenAccessor;

        public HttpRequestWrapper(ILogger<HttpRequestWrapper<TPermission>> logger, IAuthenticator authenticator, IHttpContextAccessor httpContextAccessor, IUserContextProvider userContextProvider, ICancellationTokenAccessor cancellationTokenAccessor)
        {
            this.logger = logger;
            this.authenticator = authenticator;
            this.httpContextAccessor = httpContextAccessor;
            this.userContextProvider = userContextProvider;
            this.cancellationTokenAccessor = cancellationTokenAccessor;
        }

        public async Task<ActionResult> Execute(List<TPermission> requiredPermissions, Func<Task<ActionResult>> implementation, CancellationToken cancellationToken, [CallerMemberName] string functionName = null) =>
            (await this.Execute<object>(requiredPermissions, async () => await implementation(), cancellationToken, functionName)).Result;

        public async Task<ActionResult<TResult>> Execute<TResult>(List<TPermission> requiredPermissions, Func<Task<ActionResult<TResult>>> implementation, CancellationToken cancellationToken, [CallerMemberName] string functionName = null)
        {
            this.cancellationTokenAccessor.CancellationToken = cancellationToken;
            var correlationId = $"{Guid.NewGuid():N}";
            const string eprCorrelationHeader = "x-epr-request-id";

            try
            {
                // Authentication
                if (!await this.authenticator.AuthenticateAsync(this.httpContextAccessor.HttpContext?.Request.Headers.Authorization))
                {
                    this.logger.LogDebug("Unable to authenticate user returning 401");
                    return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                }

                correlationId = this.httpContextAccessor.HttpContext?.Request?.Headers.TryGetValue(eprCorrelationHeader, out var existingHeader) ?? false
                    ? existingHeader
                    : $"{this.userContextProvider.UserId:N}|{correlationId}";
            }
            catch (Exception e)
            {
                return new StatusCodeResult(500);
            }

            using var logScope = this.logger.BeginScope("Correlation Id: {correlationId}", correlationId);
            try
            {
                this.logger.LogInformation("Entered {functionName}.", functionName);

                return await implementation();
            }
            catch (EntityNotFoundException e)
            {
                this.logger.LogError(e, "Entity not found in {functionName}.", functionName);
                return new NotFoundResult();
            }
            catch (BadRequestException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (OperationCanceledException e) when (cancellationToken.IsCancellationRequested)
            {
                // Guard here in case a cancellation exception is thrown for a reason other than that this request has been cancelled
                this.logger.LogWarning(e, "{functionName} was cancelled.", functionName);
                return new EmptyResult();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Error in {functionName}.", functionName);

                if (this.httpContextAccessor.HttpContext != null)
                {
                    var headers = this.httpContextAccessor.HttpContext.Response.Headers;

                    headers.Add("Access-Control-Expose-Headers", eprCorrelationHeader);
                    if (!headers.ContainsKey(eprCorrelationHeader))
                    {
                        headers.Add(eprCorrelationHeader, correlationId);
                    }
                }

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                this.logger.LogInformation("Leaving {functionName}.", functionName);
            }
        }
    }
}