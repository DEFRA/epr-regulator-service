using EPR.RegulatorService.Frontend.Core.Enums;

namespace EPR.RegulatorService.Frontend.Web.ViewModels;
public sealed class ManageApprovalsViewModel
{
    public ApprovalType ApprovalType { get; init; }

    public ApprovalOrganisationType ApprovalOrganisationType { get; init; }

    public string Title => ApprovalType == ApprovalType.Registration ? "Manage Registrations" : "Manage Accreditations";
}
