namespace EPR.Common.Functions.AccessControl.Interfaces;

public interface IUserContextProvider
{
    Guid UserId { get; }

    Guid CustomerOrganisationId { get; }

    Guid CustomerId { get; }

    string EmailAddress { get; }
}