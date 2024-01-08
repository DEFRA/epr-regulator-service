using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models;

[ExcludeFromCodeCoverage]
public class OrganisationSearchResult
{
    public string OrganisationName { get; set; }
    public string OrganisationType { get; set; }
    public string OrganisationId { get; set; }
    public Guid ExternalId { get; set; }
    public string CompanyHouseNumber { get; set; }
    public bool IsComplianceScheme { get; set; }
}