using EPR.RegulatorService.Frontend.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications;

[ExcludeFromCodeCoverage]
public class ApplicationsViewModel
{
    public int? PageNumber { get; set; }

    public string? SearchOrganisationName { get; set; }

    public bool IsApprovedUserTypeChecked { get; set; }

    public bool IsDelegatedUserTypeChecked { get; set; }

    public EndpointResponseStatus TransferNationResult { get; set; } = EndpointResponseStatus.NotSet;
    
    public string? TransferredOrganisationName { get; set; }
    
    public string? TransferredOrganisationAgency { get; set; }

    public EndpointResponseStatus? RejectionStatus { get; set; }

    public string? RejectedUserName { get; set; }

    public string? RejectedServiceRole { get; set; }
}