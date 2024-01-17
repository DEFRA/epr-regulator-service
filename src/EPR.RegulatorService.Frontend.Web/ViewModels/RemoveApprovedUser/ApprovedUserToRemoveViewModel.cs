using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RemoveApprovedUser;

[ExcludeFromCodeCoverage]
public class ApprovedUserToRemoveViewModel
{
    public Guid ConnExternalId { get; set; }
    public Guid OrganisationId { get; set; }

    public bool? NominationDecision { get; set; }

    public Guid PromotedPersonExternalId { get; set; }

}