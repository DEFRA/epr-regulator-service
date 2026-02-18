namespace EPR.Common.Functions.CancellationTokens
{
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public sealed class CancellationTokenAccessor : ICancellationTokenAccessor, IDisposable
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<CancellationTokenAccessor> logger;
        private CancellationTokenSource cancellationTokenSource;

        public CancellationTokenAccessor(IHttpContextAccessor httpContextAccessor, ILogger<CancellationTokenAccessor> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public CancellationToken CancellationToken
        {
            get => this.cancellationTokenSource?.Token ?? throw new NotSupportedException("Cancellation token cannot be accessed before it is set");
            set => this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value, this.httpContextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None);
        }

        public void Dispose()
        {
            if (this.cancellationTokenSource != null)
            {
                this.logger.LogDebug("Cancellation token source was disposed");
                this.cancellationTokenSource.Dispose();
            }
        }
    }
}
