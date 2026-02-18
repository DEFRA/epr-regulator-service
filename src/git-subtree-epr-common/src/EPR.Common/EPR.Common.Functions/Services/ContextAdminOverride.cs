namespace EPR.Common.Functions.Services;

using Microsoft.Extensions.Logging;

public class ContextAdminOverride
{
    private readonly ILogger logger;

    public ContextAdminOverride(ILogger<ContextAdminOverride> logger) =>
        this.logger = logger;

    public bool IsOverridden { get; private set; }

    public string EmailAddress { get; private set; }

    public Guid UserId { get; private set; }

    public Guid CustomerOrganisationId { get; private set; }

    public Guid CustomerId { get; private set; }

    public IDisposable OverrideContext(Guid userId, string emailAddress, Guid customerOrganisationId, Guid customerId)
    {
        this.IsOverridden = true;
        this.EmailAddress = emailAddress;
        this.CustomerOrganisationId = customerOrganisationId;
        this.CustomerId = customerId;
        this.UserId = userId;
        this.logger.LogDebug($"Context overridden for UserId: {this.UserId.ToString()} with Email: {this.EmailAddress}");
        return new Override(this);
    }

    private class Override : IDisposable
    {
        private readonly ContextAdminOverride contextAdminOverride;

        public Override(ContextAdminOverride contextAdminOverride) => this.contextAdminOverride = contextAdminOverride;

        public void Dispose()
        {
            this.contextAdminOverride.IsOverridden = false;
            this.contextAdminOverride.EmailAddress = string.Empty;
            this.contextAdminOverride.UserId = Guid.Empty;
            this.contextAdminOverride.CustomerOrganisationId = Guid.Empty;
            this.contextAdminOverride.CustomerId = Guid.Empty;
            this.contextAdminOverride.logger.LogDebug("Context override removed");
        }
    }
}