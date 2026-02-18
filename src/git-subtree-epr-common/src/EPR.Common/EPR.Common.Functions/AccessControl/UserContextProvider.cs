namespace EPR.Common.Functions.AccessControl;

using Interfaces;
using Services;

public class UserContextProvider : IUserContextProvider
{
    private readonly IAuthenticator authenticator;
    private readonly ContextAdminOverride contextAdminOverride;

    public UserContextProvider(IAuthenticator authenticator, ContextAdminOverride contextAdminOverride)
    {
        this.authenticator = authenticator;
        this.contextAdminOverride = contextAdminOverride;
    }

    public Guid UserId => this.contextAdminOverride.IsOverridden ? this.contextAdminOverride.UserId : this.authenticator.UserId;

    public Guid CustomerOrganisationId => this.contextAdminOverride.IsOverridden ? this.contextAdminOverride.CustomerOrganisationId : this.authenticator.CustomerOrganisationId;

    public Guid CustomerId => this.contextAdminOverride.IsOverridden ? this.contextAdminOverride.CustomerId : this.authenticator.CustomerId;

    public string EmailAddress => this.contextAdminOverride.IsOverridden ? this.contextAdminOverride.EmailAddress : this.authenticator.EmailAddress;
}