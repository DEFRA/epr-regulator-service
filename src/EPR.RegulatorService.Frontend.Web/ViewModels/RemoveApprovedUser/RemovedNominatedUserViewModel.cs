using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RemoveApprovedUser;

[ExcludeFromCodeCoverage]
public class RemovedNominatedUserViewModel
{
    public string PromotedUserName { get; set; }

    public string RemovedUserName { get; set; }

    public string OrganisationName { get; set; }
}