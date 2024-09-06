using EPR.RegulatorService.Frontend.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.UserDetailChanges;

[ExcludeFromCodeCoverage]
public class UserDetailChangesViewModel
{
    public int? PageNumber { get; set; }

    public string? SearchOrganisationName { get; set; }

    public bool IsApprovedUserTypeChecked { get; set; }

    public bool IsDelegatedUserTypeChecked { get; set; }
    
    public EndpointResponseStatus? RejectionStatus { get; set; }

    public string? RejectedUserName { get; set; }

    public string? RejectedServiceRole { get; set; }
}