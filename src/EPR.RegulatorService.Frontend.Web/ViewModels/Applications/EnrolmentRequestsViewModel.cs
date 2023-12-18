using EPR.RegulatorService.Frontend.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications;

[ExcludeFromCodeCoverage]
public class EnrolmentRequestsViewModel
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public string ReferenceNumber { get; set; }
    public string OrganisationType { get; set; }
    public BusinessAddress BusinessAddress { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public string RegisteredNation { get; set; }
    public bool IsApprovedUserAccepted { get; set; }
    public User? ApprovedUser { get; set; }
    public List<User> DelegatedUsers { get; set; }
    public EndpointResponseStatus? RejectionStatus { get; set; }
    public string? RejectedUserName { get; set; }
    public string? RejectedServiceRole { get; set; }
    public TransferBannerViewModel Transfer { get; set; }
    public EndpointResponseStatus? AcceptStatus { get; set; }
    public string AcceptedFirstName { get; set; }
    public string AcceptedLastName { get; set; }
    public string AcceptedRole { get; set; }
    public bool IsComplianceScheme { get; set; }
}
