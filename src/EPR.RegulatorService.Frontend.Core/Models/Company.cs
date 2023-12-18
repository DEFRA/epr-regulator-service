namespace EPR.RegulatorService.Frontend.Core.Models;

using CompanyDetails;

public class Company
{
    public string OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public int OrganisationTypeId { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public bool IsComplianceScheme { get; set; }
    public RegisteredAddress RegisteredAddress { get; set; }
}