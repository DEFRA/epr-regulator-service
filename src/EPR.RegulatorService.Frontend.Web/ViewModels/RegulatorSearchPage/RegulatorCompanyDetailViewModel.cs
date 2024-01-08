using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorSearchPage;

[ExcludeFromCodeCoverage]
public class RegulatorCompanyDetailViewModel
{
    public string OrganisationId { get; set; }
    public string OrganisationName { get; set; }

    public string ReferenceNumber { get; set; }
    public string OrganisationType { get; set; }
    public bool IsComplianceScheme { get; set; }
    public BusinessAddress BusinessAddress { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public ApprovedUserInformation ApprovedUsersInformation { get; set; }
    public List<CompanyUserInformation> DelegatedUsers { get; set; }
    public List<CompanyUserInformation> AdminUsers { get; set; }
    public List<CompanyUserInformation> BasicUsers { get; set; }
    public Guid ExternalOrganisationId { get; set; }
}