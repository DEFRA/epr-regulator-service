namespace EPR.RegulatorService.Frontend.Core.Models;

public class OrganisationUser
{
    public Guid PersonExternalId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}