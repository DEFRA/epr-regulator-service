namespace EPR.Common.Functions.AccessControl.MockAuthentication.Models;

public class UserInformation
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public Guid UniqueReference { get; set; }

    public string Password { get; set; }

    public Guid CustomerOrganisationId { get; set; }

    public Guid CustomerId { get; set; }
}