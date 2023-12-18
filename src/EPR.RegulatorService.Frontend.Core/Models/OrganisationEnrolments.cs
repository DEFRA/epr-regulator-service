namespace EPR.RegulatorService.Frontend.Core.Models;

public class OrganisationEnrolments
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public string OrganisationType { get; set; }
    public string OrganisationReferenceNumber { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public bool IsComplianceScheme { get; set; }
    public int NationId { get; set; }
    public string NationName { get; set; }
    public BusinessAddress BusinessAddress { get; set; }
    public List<User> Users { get; set; }
    public TransferDetailsResult TransferDetails { get; set; }
}
