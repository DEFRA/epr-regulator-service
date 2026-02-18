namespace EPR.Common.Functions.AccessControl.Interfaces;

public interface IAuthenticator
{
    public Guid UserId { get; }

    string EmailAddress { get; }

    public Guid CustomerOrganisationId { get; }

    public Guid CustomerId { get; }

    Task<bool> AuthenticateAsync(string bearerToken);
}